using Application.DTO.Model;
using Application.Interface.Service;
using Domain.Model;

namespace Application.Interface.Service
{
    public interface IExpenseService : IBaseService<Expense, ExpenseRDTO, ExpenseCDTO, ExpenseUDTO>
    {
    }
}
