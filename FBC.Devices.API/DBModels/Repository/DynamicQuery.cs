using System.Text;
using System.Linq.Dynamic.Core;

namespace FBC.Devices.API.DBModels.Repository;

public class DynamicQuery
{
    public IEnumerable<Sort>? Sort { get; set; }

    public Filter? Filter { get; set; }

    public DynamicQuery()
    {

    }

    public DynamicQuery(IEnumerable<Sort> sort, Filter? filter)
    {
        Sort = sort;
        Filter = filter;
    }
}


public class Filter
{
    public string Field { get; set; }

    public string? Value { get; set; }

    public FilterOperator Operator { get; set; }

    public FilterLogic Logic { get; set; }

    public IEnumerable<Filter>? Filters { get; set; }


    public bool CaseInsensitive { get; set; }
    //public string? Culture { get; set; } // "tr-TR", "en-US", null
    public Filter()
    {
        Field = string.Empty;
        Operator = FilterOperator.none;
    }

    public Filter(string field, FilterOperator @operator)
    {
        Field = field;
        Operator = @operator;
    }
}

public enum FilterOperator
{
    none,
    eq,
    neq,
    lt,
    lte,
    gt,
    gte,
    isnull,
    isnotnull,
    startswith,
    endswith,
    contains,
    doesnotcontain
}
public enum FilterLogic
{
    none,
    and,
    or
}
public enum SortDirection
{
    none,
    asc,
    desc
}
public static class IQueryableDynamicFilterExtensions
{

    private static readonly IDictionary<FilterOperator, string> _operators = new Dictionary<FilterOperator, string>
        {
        { FilterOperator.eq, "=" },
        { FilterOperator.neq, "!=" },
        { FilterOperator.lt, "<" },
        { FilterOperator.lte, "<=" },
        { FilterOperator.gt, ">" },
        { FilterOperator.gte, ">=" },
        { FilterOperator.isnull, "== null" },
        { FilterOperator.isnotnull, "!= null" },
        { FilterOperator.startswith, "StartsWith" },
        { FilterOperator.endswith, "EndsWith" },
        { FilterOperator.contains, "Contains" },
        { FilterOperator.doesnotcontain, "!Contains" }
    };
    private static string Normalize(string expr, bool caseInsensitive)
    {
        return caseInsensitive
            ? $"{expr}.ToLower()"
            : expr;
    }

    public static IQueryable<T> ToDynamic<T>(this IQueryable<T> query, DynamicQuery dynamicQuery)
    {
        if (dynamicQuery.Filter is not null)
            query = Filter(query, dynamicQuery.Filter);
        if (dynamicQuery.Sort is not null && dynamicQuery.Sort.Any())
            query = Sort(query, dynamicQuery.Sort);
        return query;
    }

    private static IQueryable<T> Filter<T>(IQueryable<T> queryable, Filter filter)
    {
        IList<Filter> filters = GetAllFilters(filter);
        string?[] values = filters.Select(f => f.Value).ToArray();
        string where = Transform(filter, filters);
        if (!string.IsNullOrEmpty(where) && values != null)
            queryable = queryable.Where(where, values);

        return queryable;
    }

    private static IQueryable<T> Sort<T>(IQueryable<T> queryable, IEnumerable<Sort> sort)
    {
        foreach (Sort item in sort)
        {
            if (string.IsNullOrEmpty(item.Field))
                throw new ArgumentException("Invalid Field");
            if (item.Dir == SortDirection.none)
                throw new ArgumentException("Invalid Order Type");
        }

        if (sort.Any())
        {
            string ordering = string.Join(separator: ",", values: sort.Select(s => $"{s.Field} {s.Dir}"));
            return queryable.OrderBy(ordering);
        }

        return queryable;
    }

    public static IList<Filter> GetAllFilters(Filter filter)
    {
        List<Filter> filters = new();
        GetFilters(filter, filters);
        return filters;
    }

    private static void GetFilters(Filter filter, IList<Filter> filters)
    {
        filters.Add(filter);
        if (filter.Filters is not null && filter.Filters.Any())
            foreach (Filter item in filter.Filters)
                GetFilters(item, filters);
    }

    public static string Transform(Filter filter, IList<Filter> filters)
    {
        if (string.IsNullOrEmpty(filter.Field))
            throw new ArgumentException("Invalid Field");
        if (filter.Operator == FilterOperator.none)
            throw new ArgumentException("Invalid Operator");

        int index = filters.IndexOf(filter);
        string comparison = _operators[filter.Operator];
        StringBuilder where = new();

        string fieldExpr = $"np({filter.Field})";
        string valueExpr = $"@{index}";

        if (filter.CaseInsensitive && filter.Value != null)
        {
            fieldExpr = Normalize(fieldExpr, true);
            valueExpr = $"{valueExpr}.ToLower()";
        }

        //if (!string.IsNullOrEmpty(filter.Value))
        //{
        //    if (filter.Operator == FilterOperator.doesnotcontain)
        //        where.Append($"(!np({filter.Field}).{comparison}(@{index.ToString()}))");
        //    else if (comparison is "StartsWith" or "EndsWith" or "Contains")
        //        where.Append($"(np({filter.Field}).{comparison}(@{index.ToString()}))");
        //    else
        //        where.Append($"np({filter.Field}) {comparison} @{index.ToString()}");
        //}
        if (!string.IsNullOrEmpty(filter.Value))
        {
            if (filter.Operator == FilterOperator.doesnotcontain)
                where.Append($"(!{fieldExpr}.{comparison}({valueExpr}))");
            else if (comparison is "StartsWith" or "EndsWith" or "Contains")
                where.Append($"({fieldExpr}.{comparison}({valueExpr}))");
            else
                where.Append($"{fieldExpr} {comparison} {valueExpr}");
        }
        else if (filter.Operator is FilterOperator.isnull or FilterOperator.isnotnull)
        {
            where.Append($"np({filter.Field}) {comparison}");
        }

        if (filter.Logic != FilterLogic.none && filter.Filters?.Any() == true)
        {
            if (filter.Logic == FilterLogic.none)
                throw new ArgumentException("Invalid Logic");
            return $"{where} {filter.Logic} ({string.Join(separator: $" {filter.Logic} ", value: filter.Filters.Select(f => Transform(f, filters)).ToArray())})";
        }

        return where.ToString();
    }
}


public class Sort
{
    public string Field { get; set; }

    public SortDirection Dir { get; set; }

    public Sort()
    {
        Field = string.Empty;
        Dir = SortDirection.none;
    }

    public Sort(string field, SortDirection dir)
    {
        Field = field;
        Dir = dir;
    }
}

