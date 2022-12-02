using System.Threading.Tasks;

namespace Hydrogen {

	public interface ITransactionalObject {

		event EventHandlerEx<object> Committing;
		event EventHandlerEx<object> Committed;
		event EventHandlerEx<object> RollingBack;
		event EventHandlerEx<object> RolledBack; 

		void Commit();
		
		Task CommitAsync();

		void Rollback();

		Task RollbackAsync();

	}

}