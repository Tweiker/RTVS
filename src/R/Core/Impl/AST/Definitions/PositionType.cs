﻿
namespace Microsoft.R.Core.AST.Definitions
{
    /// <summary>
    /// Describes type of text position within AST
    /// </summary>
    public enum PositionType
    {
        /// <summary>
        /// Undefined position
        /// </summary>
        Undefined,

        /// <summary>
        /// Position is within node but type 
        /// of the node is unclear
        /// </summary>
        Node,

        /// <summary>
        /// Position is inside a comment or a string
        /// </summary>
        Token,

        /// <summary>
        /// Position is inside an identifier
        /// </summary>
        Identifier,

        /// <summary>
        /// Position is inside expression
        /// </summary>
        Expression,

        /// <summary>
        /// Position is in scope but not in any of inner nodes
        /// </summary>
        Scope
    }
}