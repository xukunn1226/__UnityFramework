using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Framework.AssetManagement.AssetTreeView
{
    public enum TokenType
    {
        Variable = 0,
        StatementStart = 1,
        StatementEnd = 2,
    }

    public class TemplateFileParser
    {
        private static string separatePattern = "#\\s*([^%>]+?)\\s*#";
        private static Dictionary<string, TokenType> filterPattern = new Dictionary<string, TokenType>()
        {
            { "#TemplateTreeViewItem#", TokenType.Variable },
            { "#TemplateTreeView#", TokenType.Variable },
            { "#nameSpace#", TokenType.Variable },
            // keyWord
            { "#ColumnName#", TokenType.Variable },
            { "#LoopNum#", TokenType.Variable },
            // statement
            { "#StartColumnGUILoop#", TokenType.StatementStart },
            { "#EndColumnGUILoop#", TokenType.StatementEnd },
            { "#StartColumnStatLoop#", TokenType.StatementStart },
            { "#EndColumnStatLoop#", TokenType.StatementEnd },
        };

        private static string templateTreeviewFile =
            "/Framework/AssetManagement/AssetTreeView/Editor/TreeViewCreator/TemplateTreeView.cs.template";
        
        private Stack<int> LineParserStack = new Stack<int>();

        private TemplateFileParam param;
        public void DoGenCode(TemplateFileParam param)
        {
            param.OutputFilePath = param.OutputFilePath.Replace("Assets", ""); 
            string outputDir = UnityEngine.Application.dataPath + param.OutputFilePath +"/" ;
            string outputFile = UnityEngine.Application.dataPath + param.OutputFilePath +"/" + param.treeViewName + ".cs";
            string templateFile = UnityEngine.Application.dataPath + templateTreeviewFile;
            this.param = param;
            

            if (!Directory.Exists(outputDir))
            {
                Directory.CreateDirectory(outputDir);
            }

            using (StreamWriter writer = new StreamWriter(outputFile, false))
            {
                string[] toParserLines = File.ReadAllLines(templateFile);
                List<string> newLines = new List<string>();
                int lineIndex = 0;
                ParserLines(toParserLines, newLines, ref lineIndex);
                Debug.Log($"count:{newLines.Count}");
                foreach (var newLine in newLines)
                {
                    writer.WriteLine(newLine);
                }
            }

        }

        void ParserLines(string[] toParserLines, List<string> newLines, ref int lineIndex)
        {
            // over
            if (lineIndex >= toParserLines.Length)
            {
                return;
            }

            //
            string line = toParserLines[lineIndex];
            //var matchRes = Regex.Match(line,separatePattern);
            var matchRes = Regex.Matches(line, separatePattern);
            if (matchRes.Count > 1)
            {
                // multi var
                foreach (Match matchRe in matchRes)
                {
                    string token = matchRe.Value;
                    ParserVarToken(ref line, token);
                }

                lineIndex++;
                newLines.Add(line);

            }
            else if (matchRes.Count > 0)
            {
                if (filterPattern[matchRes[0].Value] == TokenType.Variable)
                {
                    ParserVarToken(ref line, matchRes[0].Value);
                    lineIndex++;
                    newLines.Add(line);
                }
                else
                {
                    ParserLoopStatement(toParserLines, newLines, matchRes[0].Value,ref lineIndex);
                }
            }
            else // no need to do
            {
                lineIndex++;
                newLines.Add(line);
            }

            //
            ParserLines(toParserLines,newLines,ref lineIndex);
        }

        void ParserVarToken(ref string line, string token)
        {
            var newStr = "";
            // temp
            if (token == "#LoopNum#")
            {
                int index = param.treeViewColumnName.Count - LineParserStack.Peek();
                newStr = index.ToString();
            }
            else if (token == "#ColumnName#")
            {
                int index = param.treeViewColumnName.Count - LineParserStack.Peek();
                if (index < param.treeViewColumnName.Count)
                {
                    newStr = param.treeViewColumnName[index];
                }
                else
                {
                    newStr = "outOfIndex";
                }
            }
            else if (token == "#TemplateTreeViewItem#")
            {
                newStr = param.treeViewName + "Item";
            }
            else if (token == "#TemplateTreeView#")
            {
                newStr = param.treeViewName;
            }
            else if (token == "#nameSpace#")
            {
                newStr = param.namespaceName;
            }
            else
            {
                newStr = token.Trim('#');
            }
            line = line.Replace(token, newStr);
        }

        void ParserLoopStatement(string[] toParserLines, List<string> newLines, string token, ref int lineindex)
        {
            TokenType type = filterPattern[token];
            if (type == TokenType.StatementStart)
            {
                lineindex++;
                LineParserStack.Push(lineindex);
                int loopTime = StatementTokenLoopTime(token);
                LineParserStack.Push(loopTime);

                ParserLines(toParserLines,newLines,ref lineindex);
            }
            else if (type == TokenType.StatementEnd)
            {
                int loopTime = LineParserStack.Pop();
                loopTime--;
                if (loopTime <= 0)
                {
                    LineParserStack.Pop();
                    lineindex++;
                }
                else
                {
                    int callStartIndex = LineParserStack.Pop();
                    lineindex = callStartIndex;
                    LineParserStack.Push(callStartIndex);
                    LineParserStack.Push(loopTime);
                }
            }
        }

        int StatementTokenLoopTime(string token)
        {
            if (token == "#StartColumnStatLoop#")
                return param.treeViewColumnName.Count;
            if (token == "#StartColumnGUILoop#")
                return param.treeViewColumnName.Count;
            
            return 1;
        }
    }

}
