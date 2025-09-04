using Microsoft.Extensions.Hosting;
using Microsoft.SqlServer.TransactSql.ScriptDom;
using System.ComponentModel;
using System.Xml.Linq;

namespace ScriptDom.Server
{
    public class SqlVisitor: TSqlFragmentVisitor
    {
        public IEnumerable<DataExtract> sqlDataExtracts;
        public SqlVisitor() { sqlDataExtracts = new List<DataExtract>(); }

        //public override void Visit(NamedTableReference node)
        //{
        //    GetTableName(node);
        //    base.Visit(node);
        //}


        //public override void Visit(TableReference node)
        //{
        //    NamedTableReference tbl = null;
        //    switch (node.GetType().Name)
        //    {
        //        case "NamedTableReference":
        //            tbl = node as NamedTableReference;
        //            break;
        //        case "QualifiedJoin":
        //            var join = node as QualifiedJoin;
        //            tbl = join.SecondTableReference as NamedTableReference;
        //            break;
        //    }
        //    if (tbl != null) GetTableName(tbl);
        //    base.Visit(node);
        //}

        //public override void Visit(SelectScalarExpression node)
        //{
        //    List<ColumnReferenceExpression> columnRefs = new List<ColumnReferenceExpression>();
        //    columnRefs.Add(node.Expression as ColumnReferenceExpression);
        //    GetColumnNames(columnRefs);

        //    base.Visit(node);
        //}

        public override void Visit(SelectStatement node)
        {
            QuerySpecification querySpec = node.QueryExpression as QuerySpecification;
            ProcessQuerySpec("", querySpec);

            base.Visit(node);
        }

        public override void Visit(InsertStatement node)
        {
            InsertSpecification insertSpecs = node.InsertSpecification;
            if (insertSpecs != null)
            {
                if (insertSpecs.Target != null) { GetTableName("", "insert", insertSpecs.Target as NamedTableReference); }
                GetColumnNames(insertSpecs.Columns.ToList());
            }
            if (insertSpecs.InsertSource is SelectInsertSource)
            {
                var qryExpression = insertSpecs.InsertSource as SelectInsertSource;
                ProcessBinaryQryExpression(qryExpression.Select as BinaryQueryExpression);
            }
                base.Visit(node);
        }

        private void ProcessBinaryQryExpression(BinaryQueryExpression queryExpression)
        {
            List<ColumnReferenceExpression> columnRefs = new List<ColumnReferenceExpression>();
 
            QuerySpecification querySpec;
            string expressionType;
            expressionType = queryExpression.BinaryQueryExpressionType.ToString();

            switch (queryExpression.FirstQueryExpression.GetType().Name)
            {
                case "BinaryQueryExpression":
                    var qryExp = queryExpression.FirstQueryExpression as BinaryQueryExpression;
                    querySpec = qryExp.FirstQueryExpression as QuerySpecification;
                    ProcessQuerySpec(expressionType, querySpec);

                    querySpec = qryExp.SecondQueryExpression as QuerySpecification;
                    ProcessQuerySpec(expressionType, querySpec);

                    break;
                case "QuerySpecification":
                    querySpec = queryExpression.FirstQueryExpression as QuerySpecification;
                    ProcessQuerySpec(expressionType, querySpec);
                    querySpec = queryExpression.SecondQueryExpression as QuerySpecification;
                    ProcessQuerySpec(expressionType, querySpec);
                    break;
            }
        }

        private void ProcessQuerySpec(string expressionType, QuerySpecification querySpec)
        {
            List<ColumnReferenceExpression> columnRefs = new List<ColumnReferenceExpression>();

            if (querySpec != null)
            {
                if (querySpec.FromClause != null)
                {
                    foreach (var tblRef in querySpec.FromClause.TableReferences)
                    {
                        NamedTableReference tbl = null;
                        string queryType ="";
                        switch (tblRef.GetType().Name)
                        {
                            case "NamedTableReference":
                                queryType = "select";
                                tbl = tblRef as NamedTableReference;
                                break;
                            case "QualifiedJoin":
                                queryType = "join";
                                var join = tblRef as QualifiedJoin;
                                tbl = join.SecondTableReference as NamedTableReference;
                                break;
                        }
                        if (tbl != null) GetTableName(expressionType, queryType, tbl);
                    }
                }
                var slectElements = querySpec.SelectElements.ToList();
                foreach (var column in slectElements)
                {
                    if (column is SelectScalarExpression)
                    {
                        columnRefs.Add(((SelectScalarExpression)column).Expression as ColumnReferenceExpression);
                    }
                }
                GetColumnNames(columnRefs);
            }
        }

        private void GetTableName(string expressionType, string queryType, NamedTableReference tableRef)
        {
            DataExtract de = new DataExtract();
            de.TableName = expressionType + " : " + queryType + " : " + tableRef.SchemaObject.BaseIdentifier.Value;
            sqlDataExtracts = sqlDataExtracts.Append(de);
        }

        private void GetColumnNames(List<ColumnReferenceExpression> columnRefList)
        {
            foreach (ColumnReferenceExpression columnref in columnRefList)
            {
                DataExtract de = new DataExtract();
                if (columnref.MultiPartIdentifier.Count == 2)
                {
                    de.TableName = columnref.MultiPartIdentifier.Identifiers[0].Value;
                    de.ColumnName = columnref.MultiPartIdentifier.Identifiers[1].Value;
                }
                else if (columnref.MultiPartIdentifier.Count == 1)
                {
                    de.ColumnName = columnref.MultiPartIdentifier.Identifiers[0].Value;
                }
                // ColumnType is not directly available from ColumnReferenceExpression
                // It would require additional context or schema information to determine the type
                de.ColumnType = "Unknown"; // Placeholder, as type info is not available here
                sqlDataExtracts = sqlDataExtracts.Append(de);
            }
        }


        public override void ExplicitVisit(ColumnReferenceExpression node)
        {
            //DataExtract de = new DataExtract();
            //if (node.MultiPartIdentifier.Count == 2)
            //{
            //    de.TableName = node.MultiPartIdentifier.Identifiers[0].Value;
            //    de.ColumnName = node.MultiPartIdentifier.Identifiers[1].Value;
            //    de.ColumnType = "Unknown";
            //    sqlDataExtracts = sqlDataExtracts.Append(de);
            //}
            //else if (node.MultiPartIdentifier.Count == 1)
            //{
            //    de.ColumnName = node.MultiPartIdentifier.Identifiers[0].Value;
            //}
            //base.Visit(node);
        }

    }
}
