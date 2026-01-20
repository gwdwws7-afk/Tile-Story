namespace GameFramework.BehaviorTree
{
    /// <summary>
    /// 节点的准入条件
    /// </summary>
    public abstract class BTPrecondition : BTNode
    {
        public BTPrecondition() : base(null) { }

        // Override to provide the condition check.
        public abstract bool Check();

        // Functions as a node
        public override BTResult Tick(float elapseSeconds, float realElapseSeconds)
        {
            bool success = Check();
            if (success)
            {
                return BTResult.Ended;
            }
            else
            {
                return BTResult.Running;
            }
        }
    }



    /// <summary>
    /// A pre condition that uses database.
    /// </summary>
    public abstract class BTPreconditionUseDB : BTPrecondition
    {
        protected string _dataToCheck;

        public BTPreconditionUseDB(string dataToCheck)
        {
            this._dataToCheck = dataToCheck;
        }
    }



    /// <summary>
    /// Used to check if the float data in the database is less than / equal to / greater than the data passed in through constructor.
    /// </summary>
    public class BTPreconditionFloat : BTPreconditionUseDB
    {
        public float rhs;
        private FloatFunction func;


        public BTPreconditionFloat(string dataToCheck, float rhs, FloatFunction func) : base(dataToCheck)
        {
            this.rhs = rhs;
            this.func = func;
        }

        public override bool Check()
        {
            float lhs = Database.GetData<float>(_dataToCheck);

            switch (func)
            {
                case FloatFunction.LessThan:
                    return lhs < rhs;
                case FloatFunction.GreaterThan:
                    return lhs > rhs;
                case FloatFunction.EqualTo:
                    return lhs == rhs;
            }

            return false;
        }

        public enum FloatFunction
        {
            LessThan = 1,
            GreaterThan = 2,
            EqualTo = 3,
        }
    }



    /// <summary>
    /// Used to check if the boolean data in database is equal to the data passed in through constructor
    /// </summary>
    public class BTPreconditionBool : BTPreconditionUseDB
    {
        public bool rhs;

        public BTPreconditionBool(string dataToCheck, bool rhs) : base(dataToCheck)
        {
            this.rhs = rhs;
        }

        public override bool Check()
        {
            bool lhs = Database.GetData<bool>(_dataToCheck);
            return lhs == rhs;
        }
    }



    /// <summary>
    /// Used to check if the boolean data in database is null
    /// </summary>
    public class BTPreconditionNull : BTPreconditionUseDB
    {
        private NullFunction func;

        public BTPreconditionNull(string dataToCheck, NullFunction func) : base(dataToCheck)
        {
            this.func = func;
        }

        public override bool Check()
        {
            object lhs = Database.GetData<object>(_dataToCheck);

            if (func == NullFunction.NotNull)
            {
                return lhs != null;
            }
            else
            {
                return lhs == null;
            }
        }

        public enum NullFunction
        {
            NotNull = 1,    // return true when dataToCheck is not null
            Null = 2,       // return true when dataToCheck is not null
        }
    }

    public enum CheckType
    {
        Same,
        Different
    }
}