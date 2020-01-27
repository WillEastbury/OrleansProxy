using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using OrleansSharedInterface;
using Orleans.Runtime;
using Newtonsoft.Json;
using Orleans;
using System.Collections.Generic;

namespace OrleansSharedGrains
{
    public class IndexNodeGrain<T> : Orleans.Grain, ITreeIndexNode<T> where T: class
    {
        private readonly ILogger logger;
        private readonly IPersistentState<IndexNodeState> _profile;

        public IndexNodeGrain(ILogger<IndexNodeGrain<T>> logger,[PersistentState("mgs", "myGrainStorage")] IPersistentState<IndexNodeState> profile)
        {
            this.logger = logger;
             _profile = profile;
        }

        public async Task<T> GetThisNodeData()
        {

            return JsonConvert.DeserializeObject<T>(_profile.State.Leafnode);

        }

        public async Task<bool> RemoveNodeIfNotEmpty()
        {

            _profile.State.Leafnode = null;

            // Nuke the data for this node
            if (_profile.State.ChildNodeCount == 0)
            {

                // Then clean up the empty index entries
                if(!_profile.State.IsRootNode()) await this.GrainFactory.GetGrain<ITreeIndexNode<T>>(_profile.State.ParentNode).RemoveNodeFromParent(_profile.State.Key);
                await _profile.ClearStateAsync();

            }
            else
            {
                // This parent node has other children (siblings of the deleted node) 
                // So just traverse the tree and carry on updating
                _profile.State.ChildNodeCount --;
                await _profile.WriteStateAsync();

            }
            return true;

        }

        public async Task<bool> RemoveNodeFromParent(string childId)
        {

            // Remove the cleared child node from the list and decrement the counter
            _profile.State.ChildNodes.Remove(childId);
            _profile.State.ChildNodeCount --;

            // Are there any children left on this node
            if (_profile.State.ChildNodeCount == 0)
            {

                // Then clean up the empty index entries
                if(!_profile.State.IsRootNode()) await this.GrainFactory.GetGrain<ITreeIndexNode<T>>(_profile.State.ParentNode).RemoveNodeFromParent(_profile.State.Key);
                if (_profile.State.Leafnode == null) await _profile.ClearStateAsync();

            }
            else
            {
                // This parent node has other children (siblings of the deleted node) 
                // So just traverse the tree and carry on updating
                if(!_profile.State.IsRootNode()) await this.GrainFactory.GetGrain<ITreeIndexNode<T>>(_profile.State.ParentNode).RemoveNodeFromParent(_profile.State.Key);
                await _profile.WriteStateAsync();

            }
             return true;
        }




        public async Task<IEnumerable<T>> GetTreeQueryResult(List<QueryCondition> QueryConditions, int MaxResults, int skip, string continuationtoken)
        {
            // ToDo : Fanout and Query the index
            List<T> queryResult = new List<T>(); 




            return queryResult;
        }

        public async Task<bool> AddNode(string childId, T NodeData = null)
        {

            if(_profile.State.IsNewInstance()) 
            {   
                // This is a brand new node
                _profile.State.Key = this.GetPrimaryKeyString();
                _profile.State.Level = this.GetPrimaryKeyString().Length; 
                _profile.State.ParentNode = this.GetPrimaryKeyString().Substring(0,this.GetPrimaryKeyString().Length - 1);
                _profile.State.Leafnode =  JsonConvert.SerializeObject(NodeData); 
    
            }
            else
            {
                // This node already existed
                if (_profile.State.Leafnode != null && NodeData != null) 
                {
                    throw new Exception("This leaf IndexNode already exists, please remove it and re-add it, updates are not allowed");
                }
                else
                {
                    // This is an update to an intermediate node that previously had no leaf (a branch node)
                    if (NodeData != null)
                    {
                        // We are adding a leaf node here, so add it and traverse up the tree to update the statistics 
                        _profile.State.Leafnode = JsonConvert.SerializeObject(NodeData); 
                    }
                    else
                    {
                        // Someone has added a child node in beneath us
                        _profile.State.ChildNodes.Add(_profile.State.Key);
                        _profile.State.ChildNodeCount ++; 

                    }

                }
                
            }

            // We are only notifying a parent that a child node has been added further down the index
            // Try to create the parent and link if this is not a root node
            if(!_profile.State.IsRootNode()) await this.GrainFactory.GetGrain<ITreeIndexNode<T>>(_profile.State.ParentNode).AddNode(_profile.State.Key, null);
            await _profile.WriteStateAsync();

            return true;
        }

        public async Task<string> GetParentNodeId() 
        {

            return  _profile.State.ParentNode; 
 
        }
        public async Task<IEnumerable<string>> GetChildNodeIds() 
        {

            return  _profile.State.ChildNodes; 
 
        }
    }

    [Serializable]
    public class IndexNodeState
    {
        public bool IsNewInstance()
        {
            return Key == null; 
        }

        public bool IsRootNode()
        {   
            // Root nodes have a null or zero-length parent string 
            return Level == 0;   
        }

        public string Key {get; set;} = null;

        public int Level {get; set;} = 0 ; 

        public int ChildNodeCount { get; set; } = 0;

        public string ParentNode { get; set; } = null;

        public IList<string> ChildNodes { get; set; } = new List<string>();

        public string Leafnode { get; set; } = null;
    
    }
}
