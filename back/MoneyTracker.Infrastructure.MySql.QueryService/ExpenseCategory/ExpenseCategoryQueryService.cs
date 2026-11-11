using Microsoft.EntityFrameworkCore;
using MoneyTracker.Domain.Entities;
using MoneyTracker.Domain.Entities.Categories;
using MoneyTracker.Domain.QueryService;
using MoneyTracker.Infrastructure.MySql.DDBBConection;
using MoneyTracker.Infrastructure.MySql.Mappers;

namespace MoneyTracker.Infrastructure.MySql.QueryService.ExpenseCategory;

public class ExpenseCategoryQueryService : IExpenseCategoryQueryService
{
    private readonly AppDbContext _context;

    public ExpenseCategoryQueryService(AppDbContext context)
    {
        _context = context;
    }

    /// <summary>Visibles para el usuario: las comunes (user_id NULL) y las suyas.</summary>
    private IQueryable<Dtos.ExpenseCategoryDto> Visible(int userId) =>
        _context.ExpenseCategories
            .AsNoTracking()
            .Where(x => x.UserId == null || x.UserId == userId);

    public async Task<ExpenseCategoryEntity?> GetByIdWithChildrenAsync(int id, int userId)
    {
        // 1️⃣ Cargar todas las categorías visibles (no hay CTE, EF lo hace todo en memoria)
        var dtos = await Visible(userId)
            .OrderBy(x => x.ParentId)
            .ThenBy(x => x.Name)
            .ToListAsync();

        if (!dtos.Any()) return null;

        // 2️⃣ Crear todos los nodos
        var nodes = new Dictionary<int, ExpenseCategoryEntity>();
        foreach (var d in dtos)
        {
            nodes[d.Id] = new ExpenseCategoryEntity
            {
                Id = d.Id,
                Name = d.Name,
                ParentId = d.ParentId,
                UserId = d.UserId,
                CreatedAt = d.CreatedAt,
                UpdatedAt = d.UpdatedAt,
                Children = new List<ExpenseCategoryEntity>()
            };
        }

        // 3️⃣ Enlazar padres ↔ hijos
        foreach (var node in nodes.Values)
        {
            if (node.ParentId is int pId && nodes.TryGetValue(pId, out var parent))
            {
                node.Parent = parent;
                parent.Children.Add(node);
            }
        }

        // 4️⃣ Devolver la categoría raíz que pediste
        return nodes.TryGetValue(id, out var requested)
            ? requested
            : null;
    }

    public async Task<List<ExpenseCategoryEntity>> GetAllTreeAsync(int userId)
    {
        var dtos = await Visible(userId)
            .OrderBy(x => x.ParentId).ThenBy(x => x.Name)
            .ToListAsync();

        var nodes = new Dictionary<int, ExpenseCategoryEntity>(dtos.Count);
        foreach (var d in dtos)
        {
            nodes[d.Id] = new ExpenseCategoryEntity
            {
                Id        = d.Id,
                Name      = d.Name,
                ParentId  = d.ParentId,
                UserId    = d.UserId,
                CreatedAt = d.CreatedAt,
                UpdatedAt = d.UpdatedAt,
                Children  = new List<ExpenseCategoryEntity>()
            };
        }

        var roots = new List<ExpenseCategoryEntity>();
        foreach (var node in nodes.Values)
        {
            if (node.ParentId is int pId && nodes.TryGetValue(pId, out var parent))
            {
                node.Parent = parent;         // <- si serializas, ignora Parent con [JsonIgnore]
                parent.Children.Add(node);
            }
            else
            {
                roots.Add(node);              // sin padre visible => raíz
            }
        }

        return roots;
    }

    public async Task<ExpenseCategoryEntity?> GetByIdAsync(int id, int userId)
    {
        var dto = await Visible(userId)
            .FirstOrDefaultAsync(x => x.Id == id);

        return dto?.ToDomain();
    }

    public Task<bool> ExistsAsync(int id, int userId)
    {
        return Visible(userId).AnyAsync(x => x.Id == id);
    }

}
