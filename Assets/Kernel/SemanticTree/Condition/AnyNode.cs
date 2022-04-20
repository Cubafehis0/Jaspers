﻿using System.Xml.Serialization;

namespace SemanticTree.Condition
{
    public class AnyNode : ComplexCondition
    {
        public override bool Value
        {
            get
            {
                foreach (var condition in conditionsList)
                {
                    if (condition.Value)
                    {
                        return true;
                    }
                }
                return false;
            }
        }
    }
}