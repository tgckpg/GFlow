using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

using Net.Astropenguin.Linq;

namespace GFlow.Controls
{
	using GraphElements;
	using Models.Interfaces;
	using Models.Procedure;

	class GFPathTracer
	{
		private GFDrawBoard DrawBoard;
		private IList<GFLink> AllLinks;

		public GFPathTracer( GFDrawBoard DrawBoard )
		{
			this.DrawBoard = DrawBoard;
		}

		public IList<SDataGFProcTarget> ProcsLinkFrom( GFProcedure From )
		{
			lock ( this )
			{
				AllLinks = DrawBoard.Find<GFLink>( 1 );
				return Unsafe_ProcsLinkFrom( From );
			}
		}

		public ProcManager CreateProcManager( GFProcedure From, GFProcedure To, GFProcedure OnBroken )
		{
			lock ( this )
			{
				AllLinks = DrawBoard.Find<GFLink>( 1 );
				return Unsafe_CreateProcManager( From, To, OnBroken );
			}
		}

		public ProcManager CreateProcManager( GFProcedure From )
		{
			lock( this )
			{
				AllLinks = DrawBoard.Find<GFLink>( 1 );

				GFProcedure To = From;
				GFLink Link = AllLinks.FirstOrDefault( x => x.From.Nucleus.Equals( From ) );
				HashSet<GFProcedure> LoopGuard = new HashSet<GFProcedure>() { To };
				while ( Link != null )
				{
					To = Link.To.Nucleus as GFProcedure;
					if ( To == null || LoopGuard.Contains( To ) )
						break;

					LoopGuard.Add( To );
					Link = AllLinks.FirstOrDefault( x => x.From.Nucleus.Equals( To ) );
				}

				return Unsafe_CreateProcManager( From, To, From );
			}
		}

		public GFProcedure RestoreLegacy( ProcManager PM, int SPCounter = 0 )
		{
			lock ( this )
			{
				int i = 0, j = 0;
				float OffsetY = 0 < SPCounter ? 3 * 300 * SPCounter + 50 : 0;
				GFProcedure[] GFProcs = PM.ProcList.Select( x => new GFProcedure( x ) ).ToArray();

				GFProcs.AggExec( ( a, b, s ) =>
				{
					if ( s == 0 && SPCounter == 0 )
					{
						b.IsStart = true;
					}

					if ( s < 2 )
					{
						DrawBoard.Add( b );
						b.Bounds.Y = i * 200 + 25 + OffsetY;
						b.Bounds.X = j * 400 + 50 * i + 50;

						if ( b.Properties is IProcessList ProcessList )
						{
							foreach ( IProcessNode PNode in ProcessList.ProcessNodes )
							{
								GFProcedure SubStart = RestoreLegacy( PNode.SubProcedures, SPCounter + 1 );
								if ( SubStart != null )
								{
									DrawBoard.Add( new GFLink( b.GetTransmitter( PNode ), SubStart.Receptor ) );
									SPCounter++;
								}

								// ProcList should be empty for each SubProcess in GFlow
								// as they are dynamically added at the runtime
								PNode.SubProcedures.ProcList.Clear();
							}
						}

						if ( 0 < s )
						{
							DrawBoard.Add( new GFLink( a.Transmitter, b.Receptor ) );
						}

						if ( ++i % 3 == 0 )
						{
							i = 0;
							j++;
						}
					}
				} );

				return GFProcs.FirstOrDefault();
			}
		}

		public void RestoreLinks( GFProcedure From, IList<SDataGFProcTarget> Targets )
		{
			lock ( this )
			{
				IEnumerator<SDataGFProcTarget> GEnum = Targets.GetEnumerator();

				GEnum.MoveNext();
				SDataGFProcTarget GOutput = GEnum.Current;

				if ( GOutput.Target != null )
				{
					DrawBoard.Add( new GFLink( From.Transmitter, GOutput.Target.Receptor ) );
				}

				if ( From.Properties is IProcessList LProc )
				{
					for ( int i = 0; GEnum.MoveNext(); i++ )
					{
						SDataGFProcTarget To = GEnum.Current;
						if ( To.Target != null )
						{
							DrawBoard.Add( new GFLink( From.GetTransmitter( LProc.ProcessNodes[ i ] ), To.Target.Receptor ) );
						}
					}
				}
			}
		}

		private IList<SDataGFProcTarget> Unsafe_ProcsLinkFrom( GFProcedure From )
		{
			GFLink OutputLink = AllLinks.FirstOrDefault( x => x.From.Nucleus.Equals( From ) && x.From.Dendrite00 == null );

			List<SDataGFProcTarget> GFProcs = new List<SDataGFProcTarget>() {
				new SDataGFProcTarget()
				{
					Key = "Output"
					, Target = OutputLink?.To.Nucleus as GFProcedure
				}
			};

			if ( From.Properties is IProcessList LProc )
			{
				IEnumerable<GFLink> SubProcs = AllLinks.Where( x => x.From.Nucleus.Equals( From ) && x != OutputLink ).ToArray();
				LProc.ProcessNodes.ExecEach( ( PN, i ) =>
				{
					GFProcedure GTarget = SubProcs.FirstOrDefault( x => x.From.Dendrite00.Equals( PN ) )?.To.Nucleus as GFProcedure;
					GFProcs.Add( new SDataGFProcTarget() { Key = PN.Key, Target = GTarget } );
				} );
			}

			return GFProcs;
		}

		private ProcManager Unsafe_CreateProcManager( GFProcedure From, GFProcedure To, GFProcedure OnBroken )
		{
			ProcManager PM = new ProcManager();

			List<GFLink> Chains = new List<GFLink>();
			List<GFProcedure> LoopGuard = new List<GFProcedure>();

			// Reverse tracing should be faster and easier
			GFLink Link = AllLinks.FirstOrDefault( x => x.To.Nucleus.Equals( To ) );
			while ( Link != null )
			{
				Chains.Add( Link );

				GFProcedure LFrom = ( GFProcedure ) Link.From.Nucleus;
				if ( LFrom == From )
				{
					goto Healthy;
				}

				Link = AllLinks.FirstOrDefault( x => x.To.Nucleus.Equals( LFrom ) );
			}

			if ( From == To )
			{
				PM.ProcList.Add( RealizeProcedure( From, LoopGuard ) );
			}

			return PM;

			Healthy:

			Chains.Reverse();

			PM.ProcList.Add( RealizeProcedure( From, LoopGuard ) );
			foreach ( GFLink x in Chains )
			{
				if ( x.From.SynapseType == SynapseType.BRANCH )
				{
					break;
				}
				else
				{
					PM.ProcList.Add( RealizeProcedure( ( GFProcedure ) x.To.Nucleus, LoopGuard ) );
				}
			}

			return PM;
		}

		private Procedure RealizeProcedure( GFProcedure GFP, List<GFProcedure> LoopGuard )
		{
			if ( LoopGuard.Contains( GFP ) )
			{
				throw new OverflowException( "Infinite loop detected" );
			}

			LoopGuard.Add( GFP );

			Procedure P = GFP.GetProcedure();
			if ( P is IProcessList LProc )
			{
				IProcessList SrcProcList = ( IProcessList ) GFP.Properties;
				DrawBoard.Find<GFLink>( 1 )
					.Where( x => x.From.Nucleus.Equals( GFP ) && x.From.Dendrite00 is IProcessNode )
					.ExecEach( x =>
					{
						int Index = SrcProcList.ProcessNodes.IndexOf( ( IProcessNode ) x.From.Dendrite00 );
						List<GFProcedure> SubGuard = new List<GFProcedure>( LoopGuard );
						ChainSubProcs(
							LProc.ProcessNodes[ Index ].SubProcedures.ProcList
							, ( GFProcedure ) x.To.Nucleus
							, SubGuard
						);
					} );
			}

			return P;
		}

		private void ChainSubProcs( IList<Procedure> ProcList, GFProcedure From, List<GFProcedure> LoopGuard )
		{
			ProcList.Add( RealizeProcedure( From, LoopGuard ) );

			GFLink Link = AllLinks.FirstOrDefault( x => x.From.Nucleus.Equals( From ) );
			while ( Link != null )
			{
				GFProcedure To = ( GFProcedure ) Link.To.Nucleus;
				ProcList.Add( RealizeProcedure( To, LoopGuard ) );
				Link = AllLinks.FirstOrDefault( x => x.From.Nucleus.Equals( To ) && x.From.SynapseType == SynapseType.TRUNK );
			}
		}
	}

	[DataContract]
	class SDataGFProcRel
	{
		[DataMember] public GFProcedure Source;
		[DataMember] public IList<SDataGFProcTarget> Targets;
	}

	[DataContract]
	class SDataGFProcTarget
	{
		[DataMember] public string Key;
		[DataMember] public GFProcedure Target;
	}
}