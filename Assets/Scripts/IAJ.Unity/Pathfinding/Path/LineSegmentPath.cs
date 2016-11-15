using Assets.Scripts.IAJ.Unity.Utils;
using UnityEngine;
using System;

namespace Assets.Scripts.IAJ.Unity.Pathfinding.Path
{
    public class LineSegmentPath : LocalPath
    {
        protected Vector3 LineVector;
        public LineSegmentPath(Vector3 start, Vector3 end)
        {
            this.StartPosition = start;
            this.EndPosition = end;
            this.LineVector = end - start;
        }

        public override Vector3 GetPosition(float param)
        {
			float percentage = param - (float) Math.Truncate (param);
			Vector3 result = Vector3.Lerp (this.StartPosition, this.EndPosition, percentage);
			return Vector3.Lerp (this.StartPosition, this.EndPosition, percentage);
        }

        public override bool PathEnd(float param)
        {
			float position = (float) Math.Truncate (param);
			return param - position > 0.95;
        }

        public override float GetParam(Vector3 position, float lastParam)
        {
			float param = (float) Math.Truncate (lastParam);
			return param + MathHelper.closestParamInLineSegmentToPoint (this.StartPosition, this.EndPosition, position);
        }
    }
}
