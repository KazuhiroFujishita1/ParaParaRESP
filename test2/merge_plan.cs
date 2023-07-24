using Grasshopper.Kernel;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace test2
{
    public class merge_plan : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the MyComponent1 class.
        /// </summary>
        public merge_plan()
          : base("MergePlan", "MergePlan",
              "Merge the different dampar design strategies.",
              "ParaParaRESP", "2.Edit")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("DesignCode", "DesignCode", "Input the name of design plan", GH_ParamAccess.item);
            pManager.AddGenericParameter("Description", "Description", "Input the description of design plan", GH_ParamAccess.item);
            pManager.AddGenericParameter("Plan1", "Plan1", "Input design plan of damper design", GH_ParamAccess.list);
            pManager.AddGenericParameter("Plan2", "Plan2", "Input design plan of damper design", GH_ParamAccess.list);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Plan", "Plan", "Merged design plan of damper design", GH_ParamAccess.list);
        }
        public class Member
        {
            public string MemberType { get; set; }
            public string MemberSubType { get; set; }
            public List<string> Floor { get; set; }
            public List<string> Frame { get; set; }
            public List<string> Axis { get; set; }
            public List<ActionQueries> ActionQueries { get; set; }
        }
        public class ActionQueries
        {
            public string QueryType { get; set; }
            public string Name { get; set; }
            public string Value { get; set; }
            public string ValueType { get; set; }
        }
        public class Plan
        {
            public string Code { get; set; }
            public string Description { get; set; }
            public List<Member> Members { get; set; }
        }
        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            ///Resp-D member jsonオブジェクトの作成
            List<dynamic> plans1 = new List<dynamic>();
            List<dynamic> plans2 = new List<dynamic>();
            string code="";
            string description = "";
            DA.GetData("DesignCode", ref code);
            DA.GetData("Description", ref description);
            DA.GetDataList("Plan1", plans1);
            DA.GetDataList("Plan2", plans2);
            int length1 = plans1.Count;
            int length2 = plans2.Count;
            //plan数の小さいほうをマージ後のplan数として採用
            int length = Math.Max(length1, length2);

            //全てのプラン情報を格納する構造体を作成
            List<object> allplan = new List<object>();
            //受け取った全てのplans情報を展開
            if (plans1 != null)
            {

                for (int i = 0; i < length; i++)
                {

                    //どちらかのリストが大きい場合、小さい方は同じものを読み込む
                    object temp;
                    object temp2;
                    if (i > length1-1)
                    {
                        temp = plans1[length1 - 1];
                    }
                    else
                    {
                        temp = plans1[i];
                    }
                    if (i > length2 - 1)
                    {
                        temp2 = plans2[length2 - 1];
                    }
                    else
                    {
                        temp2 = plans2[i];
                    }

                    // JSON形式に変換
                    var tem = JsonConvert.SerializeObject(temp, Formatting.Indented);
                    var tem2 = JsonConvert.SerializeObject(temp2, Formatting.Indented);
                    // JSON形式データの要素を抽出
                    JObject jObject = JObject.Parse(tem);
                    JObject jObject2 = JObject.Parse(tem2);

                    Plan plan1 = new Plan
                    {
                        Code = code,
                        Description = description,
                        Members = new List<Member>()
                    };
                    //plan1を代入
                    foreach (var member in jObject["Value"]["Members"])
                    {
                        Member member1 = new Member
                        {
                            MemberType = (string)member["MemberType"],
                            MemberSubType = (string)member["MemberSubType"],
                            Floor = ((JArray)member["Floor"]).ToObject<List<string>>(),
                            Frame = ((JArray)member["Frame"]).ToObject<List<string>>(),
                            Axis = ((JArray)member["Axis"]).ToObject<List<string>>(),
                            ActionQueries = new List<ActionQueries>()
                        };
                        ActionQueries query = new ActionQueries
                        {
                            QueryType = (string)member["ActionQueries"][0]["QueryType"],
                            Name = (string)member["ActionQueries"][0]["Name"],
                            Value = (string)member["ActionQueries"][0]["Value"],
                            ValueType = (string)member["ActionQueries"][0]["ValueType"]
                        };
                        // 各データをクラスに代入する
                        member1.ActionQueries.Add(query);
                        plan1.Members.Add(member1);
                    }
                    //plan2を代入
                    foreach (var member in jObject2["Value"]["Members"])
                    {
                        Member member2 = new Member
                        {
                            MemberType = (string)member["MemberType"],
                            MemberSubType = (string)member["MemberSubType"],
                            Floor = ((JArray)member["Floor"]).ToObject<List<string>>(),
                            Frame = ((JArray)member["Frame"]).ToObject<List<string>>(),
                            Axis = ((JArray)member["Axis"]).ToObject<List<string>>(),
                            ActionQueries = new List<ActionQueries>()
                        };
                        ActionQueries query = new ActionQueries
                        {
                            QueryType = (string)member["ActionQueries"][0]["QueryType"],
                            Name = (string)member["ActionQueries"][0]["Name"],
                            Value = (string)member["ActionQueries"][0]["Value"],
                            ValueType = (string)member["ActionQueries"][0]["ValueType"]
                        };
                        // 各データをクラスに代入する
                        member2.ActionQueries.Add(query);
                        plan1.Members.Add(member2);
                    }
                    allplan.Add(plan1);
                };
                };
            DA.SetDataList(0, allplan);

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
                return test2.Properties.Resources.merge;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("d163bb40-4439-48ab-b315-49d27d65eeee"); }
        }
    }
}