using Assets.Scripts.IAJ.Unity.Pathfinding.Path;
using System;
using Assets.Scripts.IAJ.Unity.Utils;
using UnityEngine;

namespace Assets.Scripts.IAJ.Unity.Movement.DynamicMovement
{
    public class DynamicFollowPath : DynamicArrive
    {
		public override string Name
		{
			get { return "FollowPath"; }
		}

        public Path Path { get; set; }
		// Used to do a foward check for a new position.
        public float PathOffset { get; set; }

		// The current param for the current local path
		// this one is reseted after we finish a local path
        public float CurrentParam { get; set; }

        private MovementOutput EmptyMovementOutput { get; set; }

		public DynamicFollowPath()
        {
			// Its equal every time so we create it here.
			this.EmptyMovementOutput = new MovementOutput();
        }

        public override MovementOutput GetMovement()
        {
			
			// We check if we reached the end of the destiny here
			// if we do then put all velocities at 0 and return base method.
			if (this.Path.PathEnd (this.CurrentParam)) {
				this.MaxAcceleration = 0.0f;
				this.MovingTarget.velocity = Vector3.zero;
				this.Character.velocity = Vector3.zero;
				return this.EmptyMovementOutput;
			}

			this.CurrentParam = this.Path.GetParam (this.Character.position, this.CurrentParam);
			this.Target.position = this.Path.GetPosition (this.CurrentParam + this.PathOffset);
		

			return base.GetMovement ();
        }

		public void prepare(KinematicData character, Path path)
		{
			this.Target = new KinematicData();
			this.Character = character;
			this.Path = path;
			//don't forget to set all properties
			//arrive properties
			this.CurrentParam = 0.0f;
			this.PathOffset = 1.0f;
			this.MaxAcceleration = 15.0f;
		}
    }
}
