using Assets.Scripts.IAJ.Unity.DecisionMaking.GOB;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Scripts.GameManager
{
    public class FutureStateWorldModel : WorldModel
    {
        public FutureStateWorldModel(List<Action> actions) : base(actions)
        {
        }

        public FutureStateWorldModel(WorldModel parent) : base(parent)
        {
        }

        public override WorldModel GenerateChildWorldModel()
        {
            return new FutureStateWorldModel(this);
        }

        public override bool IsTerminal()
        {
            int HP = (int)this.GetProperty(Properties.HP);
            float time = (float)this.GetProperty(Properties.TIME);
            int money = (int)this.GetProperty(Properties.MONEY);

            return HP == 0 ||  time >= 100 || money == 25;
        }

        public override float GetScore()
        {
            int money = (int)this.GetProperty(Properties.MONEY);

            if (money == 25)
            {
                return 1.0f;
            }
            else return 0.0f;
        }

    }
}
