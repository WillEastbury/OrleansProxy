using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace OrleansSharedInterface
{
    public interface ITreeIndexNode<T> : Orleans.IGrainWithStringKey where T: class
    {
        Task<string> GetParentNodeId();
        Task<IEnumerable<string>> GetChildNodeIds();
        Task<T> GetThisNodeData();
        Task<IEnumerable<T>> GetTreeQueryResult(List<QueryCondition> QueryConditions, int MaxResults, int skip, string continuationtoken);      
        Task<bool> AddNode(string childId, T NodeData);
        Task<bool> RemoveNodeIfNotEmpty();
        Task<bool> RemoveNodeFromParent(string childId);
    } 

    public class QueryCondition
    {
        public string FilterField = ""; 
        public object FilterValue = ""; 
        public QueryConditionPredicate Predicate = QueryConditionPredicate.EqualTo;   
    }

    public enum QueryConditionPredicate
    
    {
        IsNull,
        EqualTo,
        NotEqualTo,
        GreaterThan,
        LessThan,
        Contains, 
        NotContains,
        GreaterThanOrEqualTo,
        LessThanOrEqualTo,
        Missing
    }

}
