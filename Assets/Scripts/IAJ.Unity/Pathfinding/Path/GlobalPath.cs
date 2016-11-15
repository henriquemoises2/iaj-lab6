using System.Collections.Generic;
using Assets.Scripts.IAJ.Unity.Utils;
using RAIN.Navigation.Graph;
using UnityEngine;
using System;

namespace Assets.Scripts.IAJ.Unity.Pathfinding.Path
{
    public class GlobalPath : Path
    {
        public List<NavigationGraphNode> PathNodes { get; protected set; }
        public List<Vector3> PathPositions { get; protected set; } 
        public bool IsPartial { get; set; }
        public float Length { get; set; }
        public List<LocalPath> LocalPaths { get; protected set; } 


        public GlobalPath()
        {
            this.PathNodes = new List<NavigationGraphNode>();
            this.PathPositions = new List<Vector3>();
            this.LocalPaths = new List<LocalPath>();
        }

        public void CalculateLocalPathsFromPathPositions(Vector3 initialPosition)
        {
            Vector3 previousPosition = initialPosition;
            for (int i = 0; i < this.PathPositions.Count; i++)
            {

                if (!previousPosition.Equals(this.PathPositions[i]))
                {
                    this.LocalPaths.Add(new LineSegmentPath(previousPosition, this.PathPositions[i]));
                    previousPosition = this.PathPositions[i];
                }
            }
        }

        public override float GetParam(Vector3 position, float previousParam)
        {
			float postion = (float) Math.Truncate (previousParam);
			int pos = (int) postion;
			LocalPath currentPath = this.LocalPaths [pos];
			return currentPath.GetParam (position, previousParam);
        }

        public override Vector3 GetPosition(float param)
        {
			float postionInt = (float) Math.Truncate (param);
			int pos = (int) postionInt;
			LocalPath currentPath = this.LocalPaths [pos];
			return currentPath.GetPosition (param);
        }

        public override bool PathEnd(float param)
        {
			bool result = false;
			int position = (int) Math.Truncate (param);
			int count = this.LocalPaths.Count - 1;
			if (position < count)
				result = false;
			else
				return true;
			LocalPath path = this.LocalPaths [position];
			return result && path.PathEnd(param);
        }
    }
}
