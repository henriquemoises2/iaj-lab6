using Assets.Scripts.IAJ.Unity.DecisionMaking.GOB;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.IAJ.Unity.DecisionMaking.MCTS
{
    public class MCTS
    {
        public const float C = 1.4f;
        public bool InProgress { get; private set; }
        public int MaxIterations { get; set; }
        public int MaxIterationsProcessedPerFrame { get; set; }
        public int MaxPlayoutDepthReached { get; private set; }
        public int MaxSelectionDepthReached { get; private set; }
        public float TotalProcessingTime { get; private set; }
        public MCTSNode BestFirstChild { get; set; }
        public List<GOB.Action> BestActionSequence { get; private set; }


        private int CurrentIterations { get; set; }
        private int CurrentIterationsInFrame { get; set; }
        private int CurrentDepth { get; set; }

        private CurrentStateWorldModel CurrentStateWorldModel { get; set; }
        private MCTSNode InitialNode { get; set; }
        private System.Random RandomGenerator { get; set; }
        
        

        public MCTS(CurrentStateWorldModel currentStateWorldModel)
        {
            this.InProgress = false;
            this.CurrentStateWorldModel = currentStateWorldModel;
            this.MaxIterations = 100;
            this.MaxIterationsProcessedPerFrame = 10;
            this.RandomGenerator = new System.Random();
        }


        public void InitializeMCTSearch()
        {
            this.MaxPlayoutDepthReached = 0;
            this.MaxSelectionDepthReached = 0;
            this.CurrentIterations = 0;
            this.CurrentIterationsInFrame = 0;
            this.TotalProcessingTime = 0.0f;
            this.CurrentStateWorldModel.Initialize();
            this.InitialNode = new MCTSNode(this.CurrentStateWorldModel)
            {
                Action = null,
                Parent = null,
                PlayerID = 0
            };
            this.InProgress = true;
            this.BestFirstChild = null;
            this.BestActionSequence = new List<GOB.Action>();
            
            //while (/*whitin computation budget*/  this.CurrentIterations < this.MaxIterations )
            //{
            //    MCTSNode nextNode = Selection(this.InitialNode);
            //    Reward reward = Playout(nextNode.State);
            //    Backpropagate(nextNode, reward);
            //}

            //this.InitialNode.Action = this.CurrentStateWorldModel.GetExecutableActions()[RandomGenerator.Next(0, this.CurrentStateWorldModel.GetExecutableActions().Length - 1)];
            //Debug.Log(this.InitialNode.Action.ToString());
        }

        public GOB.Action Run()
        {
            if (this.CurrentIterations < this.MaxIterations)
            {
                MCTSNode selectedNode;
                Reward reward;

                var startTime = Time.realtimeSinceStartup;
                this.CurrentIterationsInFrame = 0;

                selectedNode = Selection(this.InitialNode);
                reward = Playout(selectedNode.State);
                Backpropagate(selectedNode, reward);
                return selectedNode.Action;
            }
            else
            {
                return null;
            }
                
        }

        private MCTSNode Selection(MCTSNode initialNode)
        {
            // GOB.Action nextAction;
            MCTSNode currentNode = initialNode;

            while (!currentNode.State.IsTerminal())
            {
                foreach (var action in currentNode.State.GetExecutableActions())
                {
                    Expand(currentNode, action);
                }
                currentNode = BestUCTChild(currentNode);
            }
            return currentNode;
                
        }

        private Reward Playout(WorldModel initialPlayoutState)
        {
            WorldModel currentState = initialPlayoutState;
            while (!currentState.IsTerminal())
            {
                GOB.Action action = currentState.GetExecutableActions()[RandomGenerator.Next(0, currentState.GetExecutableActions().Length - 1)];
                action.ApplyActionEffects(currentState);
            }

            this.CurrentIterations++;
            Reward reward = new Reward();
            reward.Value = currentState.GetScore();
            return reward;
        }

        private void Backpropagate(MCTSNode node, Reward reward)
        {
            while (node != null)
            {
                node.N += 1;
                node.Q += reward.Value; 
                node = node.Parent; 
            }
        }

        private MCTSNode Expand(MCTSNode parent, GOB.Action action)
        {
            MCTSNode parentNodeCopy = parent;
            action.ApplyActionEffects(parentNodeCopy.State);
            MCTSNode child = new MCTSNode(parentNodeCopy.State);
            child.Parent = parent;
            parent.ChildNodes.Add(child);
            return child;
        }

        //gets the best child of a node, using the UCT formula
        private MCTSNode BestUCTChild(MCTSNode node)
        {
            MCTSNode bestChild = node.ChildNodes[0];
            double UCTValue, MaxUCTValue = 0;
            foreach (MCTSNode child in node.ChildNodes)
            {
                UCTValue = child.State.GetScore() + C * Math.Sqrt(Math.Log((node.N != 0 ? node.N : 0)/ node.Parent.N != 0 ? node.Parent.N : 0); // Hammered because couldn't figure out how a node was already visited

                if(UCTValue > MaxUCTValue)
                {
                    bestChild = child;
                }                
            }
            return bestChild;
        }

        //this method is very similar to the bestUCTChild, but it is used to return the final action of the MCTS search, and so we do not care about
        //the exploration factor
        private MCTSNode BestChild(MCTSNode node)
        {
            //float max = 0;
            //MCTSNode retChild = node;

            //foreach (MCTSNode child in node.ChildNodes) {
            //    if (child.Q > max)
            //    {
            //        max = child.Q;
            //        retChild = child;
            //        Debug.Log(child.ToString());
            //    }
            //}
            //Debug.Log(retChild.ToString());
            //return retChild;
            throw new NotImplementedException();
        }
    }
}
