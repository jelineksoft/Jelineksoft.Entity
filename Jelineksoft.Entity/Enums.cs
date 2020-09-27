namespace Jelineksoft.Entity
{
    public enum AggregationFunctionsEnum
    {
        Sum,
        Min,
        Max,
        Avg,
        Count
    }

    public enum MathOperatorEnum
    {
        Plus,
        Minus,
        Multiple,
        Divide
    }
    
    public enum PropertyCompareOperatorEnum
    {
        /// <summary>
        /// Rovno 
        /// </summary>
        IsEqual,
        /// <summary>
        /// Nerovno 
        /// </summary>
        IsNotEqual,
        /// <summary>
        /// Menší nebo rovno 
        /// </summary>
        IsLessOrEqual,
        /// <summary>
        /// Menší
        /// </summary>
        IsLess,
        /// <summary>
        /// Větší nebo rovno 
        /// </summary>
        IsGreaterOrEqual,
        /// <summary>
        /// Větší 
        /// </summary>
        IsGreater,
        /// <summary>
        /// Začíná na
        /// </summary>
        Like        
    }
    
    public enum WhereMergeOperatorEnum
    {
        And,
        Or
    }
    
    /// <summary>
    /// Typ spojení JOINu.
    /// </summary>
    public enum JoinTypeEnum
    {
        InnerJoin,
        LeftJoin,
        RightJoin
    }

    public enum OrderByEnum
    {
        Asc,
        Desc
    }
}