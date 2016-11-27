using Assets.Scripts.GameManager;
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
            // Initializes the MTCS class with the current game state
            // This will be used to create the root node 
            this.InProgress = false;
            this.CurrentStateWorldModel = currentStateWorldModel;
            this.MaxIterations = 100;
            this.MaxIterationsProcessedPerFrame = 10;
            this.RandomGenerator = new System.Random();
        }

        // Runs every couple frames
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

            // No best child node nor action sequence at this point. They will be added during the algorithm execution
            this.BestFirstChild = null;
            this.BestActionSequence = new List<GOB.Action>();
        }

        public GOB.Action Run()
        {
            MCTSNode selectedNode;
            Reward reward;
            this.CurrentIterationsInFrame = 0;
            var startTime = Time.realtimeSinceStartup;
            

            // This will be used to limit the time and memory used in the search. 
            while (this.CurrentIterationsInFrame < this.MaxIterationsProcessedPerFrame && this.CurrentIterations < this.MaxIterations)
            {
                // Selection: Select best node for the playout fase.
                selectedNode = Selection(this.InitialNode);

                // Playout: Drills randomly until a final state is achieved
                reward = Playout(selectedNode.State);
                
                // Backpropagate: Updates all the parent nodes until the root with the value obtained in the Playout phase 
                Backpropagate(selectedNode, reward);
                this.CurrentIterations += 1;
                this.CurrentIterationsInFrame += 1;
            }
            this.TotalProcessingTime += Time.realtimeSinceStartup - startTime;

            if (this.CurrentIterations >= this.MaxIterations)
                this.InProgress = false;

            this.BestFirstChild = BestChild(InitialNode);
            this.BestActionSequence.Clear();
            var node = this.BestFirstChild;
            while(true)
            {
                if (node == null || node.State.IsTerminal())
                    break;
                this.BestActionSequence.Add(node.Action);
                node = BestChild(node);
            }

            return this.BestFirstChild.Action;
        }

        private MCTSNode Selection(MCTSNode initialNode)
        {
            var node = initialNode;
            int selectionReach = 0;
            while (!node.State.IsTerminal())
            {
                if (node.ChildNodes.Count < node.State.GetExecutableActions().Length)
                {
                    return Expand(node, node.State.GetNextAction());
                }
                else
                {
                    selectionReach += 1;
                    node = BestUCTChild(node);
                }
            }
            if (selectionReach > MaxSelectionDepthReached)
                MaxSelectionDepthReached = selectionReach;

            return node;
        }

        private Reward Playout(WorldModel initialPlayoutState)
        {
            //Debug.Log("Playout MCTS");
            WorldModel roll = initialPlayoutState.GenerateChildWorldModel();

            int playoutReach = 0;
            // Drills randomly (through a random child) until a terminal state is achieved
            while (!roll.IsTerminal())
            {
                // Gets a random action
                GOB.Action action = roll.GetExecutableActions()[RandomGenerator.Next(0, roll.GetExecutableActions().Length - 1)];
                action.ApplyActionEffects(roll);
                playoutReach += 1;
            }

            if (playoutReach > MaxPlayoutDepthReached)
                MaxPlayoutDepthReached = playoutReach;

            // Gets the reward of that branch (the Score of that reached final state) 
            Reward reward = new Reward();
            reward.Value = roll.GetScore();
            return reward;
        }

        private void Backpropagate(MCTSNode node, Reward reward)
        {
            //Debug.Log("Backpropagating MCTS");
            // Drills up and updates each parent until the root with the reward value of the final node
            while (node != null)
            {
                node.N += 1;
                node.Q += reward.Value; 
                node = node.Parent; 
            }
        }

        private MCTSNode Expand(MCTSNode parent, GOB.Action action)
        {
            WorldModel futureState = parent.State.GenerateChildWorldModel();
            action.ApplyActionEffects(futureState);
            MCTSNode child = new MCTSNode(futureState);
            child.Parent = parent;
            child.Action = action;
            parent.ChildNodes.Add(child);
            return child;
        }

        //gets the best child of a node, using the UCT formula
        private MCTSNode BestUCTChild(MCTSNode node)
        {
            MCTSNode bestChild = null;
            double UCTValue, MaxUCTValue = float.MinValue;

            foreach (MCTSNode child in node.ChildNodes)
            {
                UCTValue = child.Q/child.N + C * Math.Sqrt(Math.Log(child.Parent.N) / node.N);

                if (UCTValue > MaxUCTValue)
                {
                    bestChild = child;
                    MaxUCTValue = UCTValue;
                }
            }
            return bestChild;
        }

        //this method is very similar to the bestUCTChild, but it is used to return the final action of the MCTS search, and so we do not care about
        //the exploration factor
        private MCTSNode BestChild(MCTSNode node)
        {
            MCTSNode bestChild = null;
            double UCTValue, MaxUCTValue = float.MinValue;

            foreach (MCTSNode child in node.ChildNodes)
            {
                UCTValue = child.Q/child.N + Math.Sqrt(Math.Log(child.Parent.N) / node.N);

                if (UCTValue > MaxUCTValue)
                {
                    bestChild = child;
                    MaxUCTValue = UCTValue;
                }
            }
            return bestChild;
        }
    }
}
