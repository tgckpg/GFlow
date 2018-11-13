using System;
using System.Collections.Generic;
using System.Linq;
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

		public ProcManager CreateProcManager( GFProcedure From, GFProcedure To, GFProcedure OnBroken )
		{
			lock ( this )
			{
				AllLinks = DrawBoard.Find<GFLink>( 1 );
				return Unsafe_CreateProcManager( From, To, OnBroken );
			}
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
				DrawBoard.Find<GFLink>( 1 )
					.Where( x => x.From.Nucleus.Equals( GFP ) && x.From.Dendrite00 is IProcessNode )
					.ExecEach( x =>
					{
						string PKey = ( ( IProcessNode ) x.From.Dendrite00 ).Key;
						List<GFProcedure> SubGuard = new List<GFProcedure>( LoopGuard );
						ChainSubProcs(
							LProc.ProcessNodes.First( b => b.Key == PKey ).SubProcedures.ProcList
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
				Link = AllLinks.FirstOrDefault( x => x.From.Nucleus.Equals( To ) );
			}
		}

	}
}