using Grasshopper.Kernel;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace test2
{
    public class member : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the MyComponent1 class.
        /// </summary>
        public member()
          : base("SelectMember", "SelectMember",
              "Input Member information",
              "ParaParaRESP", "0.Setting")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            int idx;
            pManager.AddTextParameter("MemberType", "MemberType", "Input member infomation.", GH_ParamAccess.item);
            idx = pManager.AddTextParameter("MemberSubType", "MemberSubType", "Input member infomation.", GH_ParamAccess.item);
            pManager[idx].Optional = true;
            idx = pManager.AddGenericParameter("Floor", "Floor", "Input Floor information", GH_ParamAccess.list);
            pManager[idx].Optional = true;
            idx = pManager.AddGenericParameter("Frame", "Frame", "Input Frame information", GH_ParamAccess.list);
            pManager[idx].Optional = true;
            idx = pManager.AddGenericParameter("Axis", "Axis", "Input Axis information", GH_ParamAccess.list);
            pManager[idx].Optional = true;
           // pManager.AddTextParameter("QueryType", "QueryType", "Input action.", GH_ParamAccess.item);
    // pManager.AddTextParameter("Name", "Name", "Input action.", GH_ParamAccess.item);
       //     pManager.AddTextParameter("Value", "Value", "Input action.", GH_ParamAccess.item);
       //     pManager.AddTextParameter("ValueType", "ValueType", "Input action.", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Target", "Target", "Connect to the placeincrement component", GH_ParamAccess.list);
        }

        //位置データの変換メソッド
        public static List<List<string>> ConvertList(List<string> originalList)
        {
            List<List<string>> convertedList = new List<List<string>>();

            string currentStart = null;
            string currentEnd = null;

            foreach (string element in originalList)
            {
                if (currentStart == null)
                {
                    currentStart = element;
                    currentEnd = element;
                }
                else if (IsSuccessor(currentEnd, element))
                {
                    currentEnd = element;
                }
                else
                {
                    AddToList(convertedList, currentStart, currentEnd);
                    currentStart = element;
                    currentEnd = element;
                }
            }

            if (currentStart != null && currentEnd != null)
            {
                AddToList(convertedList, currentStart, currentEnd);
            }

            return convertedList;
        }

        public static bool IsSuccessor(string prevElement, string currentElement)
        {
            int prevNumber = 0;
            int currentNumber = 0;
            if (prevElement.Contains("F"))
            {
                prevNumber = int.Parse(prevElement.Substring(0, prevElement.Length - 1));
                currentNumber = int.Parse(currentElement.Substring(0, currentElement.Length - 1));
            }
            else if (prevElement.Contains("X"))
            {
                prevNumber = int.Parse(prevElement.Substring(1));
                currentNumber = int.Parse(currentElement.Substring(1));
            }
            else if (prevElement.Contains("Y"))
            {
                prevNumber = int.Parse(prevElement.Substring(1));
                currentNumber = int.Parse(currentElement.Substring(1));
            }

            return currentNumber == prevNumber + 1;
        }

        public static void AddToList(List<List<string>> list, string start, string end)
        {
            List<string> sublist = new List<string>();

            if (start.EndsWith("F"))
            {
                sublist.Add(start);
                sublist.Add(end);
            }
            else if (start.StartsWith("X"))
            {
                sublist.Add(start);
                sublist.Add(end);
            }
            else if (start.StartsWith("Y"))
            {
                sublist.Add(start);
                sublist.Add(end);
            }

            list.Add(sublist);
        }


        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            // grasshopperから情報を読み込み
            var floor = new List<string>();
            var frame = new List<string>();
            var axis = new List<string>();
            string membertype = "";
            string membersubtype = "";
            string querytype = "";
            string name = "";
            string value = "";
            string valuetype = "";

            if ((!DA.GetDataList("Floor", floor)))
            {
                floor.Add("");
                floor.Add("");
            }

            if ((!DA.GetDataList("Frame", frame)))
            {
                frame.Add("");
                frame.Add("");
            }
            if ((!DA.GetDataList("Axis", axis)))
            {
                axis.Add("");
                axis.Add("");
            }

            DA.GetData("MemberType", ref membertype);
            if (Params.Input[1] != null)
            {
                DA.GetData("MemberSubType", ref membersubtype);
            }
            else
            {
                membersubtype = "";
            }
            //DA.GetData("QueryType", ref querytype);
            //DA.GetData("Name", ref name);
            //DA.GetData("Value", ref value);
            //DA.GetData("ValueType", ref valuetype);

            //位置データをRESP-Dscriptの形式に変換
            List<List<string>> converted_floor = ConvertList(floor);
            List<List<string>> converted_frame = ConvertList(frame);
            List<List<string>> converted_axis = ConvertList(axis);

            // メンバー情報を作成
            var combination_num = converted_floor.Count * converted_frame.Count * converted_axis.Count;
            List<List<List<string>>> combination = new List<List<List<string>>>();
            for (int i = 0; i <= converted_floor.Count - 1; i++)
            {
                for (int j = 0; j <= converted_frame.Count - 1; j++)
                {
                    for (int k = 0; k <= converted_axis.Count - 1; k++)
                    {
                        combination.Add(new List<List<string>> { converted_floor[i], converted_frame[j], converted_axis[k] });
                    }
                }

            }

            //全てのメンバー情報を構造体に挿入
            List<object> members = new List<object>();
            for (int i = 0; i <= combination_num - 1; i++)
            {
                var temp = combination[i];
                var member = new Member
                {
                    MemberType = membertype,
                    MemberSubType = membersubtype,
                    Floor = temp[0],
                    Frame = temp[1],
                    Axis = temp[2],
                    ActionQueries = new List<ActionQuery> {
                    new ActionQuery {
                        QueryType = querytype,
                        Name = name,
                        Value = value,
                        ValueType = valuetype
                    }
                }
                };
                members.Add(member);
            }

            // grasshopperへ出力
            DA.SetDataList(0, members);
           
        }



// JSONの型定義
        public class Member
        {
            public string MemberType { get; set; }
            public string MemberSubType { get; set; }
            public List<string> Floor { get; set; }
            public List<string> Frame { get; set; }
            public List<string> Axis { get; set; }
            public List<ActionQuery> ActionQueries { get; set; }
        }
        public class ActionQuery
        {
            public string QueryType { get; set; }
            public string Name { get; set; }
            public string Value { get; set; }
            public string ValueType { get; set; }
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                //You can add image files to your project resources and access them like this:
                // return Resources.IconForThisComponent;
                return null;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("238b960c-69ce-42b5-8a65-295e035046a6"); }
        }
    }
}