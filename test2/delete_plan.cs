using Grasshopper.Kernel;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO;

namespace test2
{
    public class delete_plan : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the MyComponent1 class.
        /// </summary>
        public delete_plan()
          : base("DeleteMember", "DeleteMember",
              "Eliminate the existing brace based on the information of member.",
              "ParaParaRESP", "1.Plan")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            int idx = 0;

            pManager.AddTextParameter("DesignCode", "DesignCode", "Input the code name.", GH_ParamAccess.item);
            pManager.AddTextParameter("Description", "Description", "Input the discription.", GH_ParamAccess.item);
            pManager[idx].Optional = true;
            pManager.AddGenericParameter("Target", "Target", "Input the recipe of damper design", GH_ParamAccess.list);
            //pManager.AddGenericParameter("Strategy", "Strategy", "Input the strategy of damper design", GH_ParamAccess.item);
            //pManager.AddGenericParameter("Name", "Name", "Input the method of operation of damper design", GH_ParamAccess.item);
            //pManager.AddGenericParameter("Value", "Value", "Input the method of operation of damper design", GH_ParamAccess.item);
            //pManager.AddGenericParameter("ValueType", "ValueType", "Input the method of operation of damper design", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Plan", "Plan", "Connect to the Make RESP-D script component", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        /// 
        //JSONの型定義
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

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            ///Resp-D member jsonオブジェクトの作成
            List<dynamic> damper_recipe = new List<dynamic>();
            DA.GetDataList("Target", damper_recipe);
            int length = damper_recipe.Count;

            //Plan情報を作成
            string code = "";
            string discription = "";
            DA.GetData("DesignCode", ref code);
            DA.GetData("Description", ref discription);
            string method = "Delete";
            string name = "Mark";
            string value = "";
            string valuetype = "string";
            //DA.GetData("Strategy", ref method);
            //DA.GetData("Name", ref name);
            //DA.GetData("Value", ref value);
            //DA.GetData("ValueType", ref valuetype);

            var plans = new Plan
            {
                Code = code,
                Description = discription,
                Members = new List<Member>()
            };

            //受け取った全てのMember情報を展開
            if (damper_recipe != null)
            {
                for (int i = 0; i < length; i++)
                {
                    var temp = damper_recipe[i];
   //                 Grasshopper.Kernel.Types.GH_ObjectWrapper temp = new Grasshopper.Kernel.Types.GH_ObjectWrapper();
                    // JSON形式に変換
                    var tem = JsonConvert.SerializeObject(temp, Formatting.Indented);
                    // JSON形式データの要素を抽出
                    JObject jObject = JObject.Parse(tem);
                    // MemberのデータをPlanのデータに追加
                    plans.Members.Add(new Member
                    {
                        MemberType = (string)jObject["Value"]["MemberType"],
                        MemberSubType = (string)jObject["Value"]["MemberSubType"],
                        Floor = ((JArray)jObject["Value"]["Floor"]).ToObject<List<string>>(),
                        Frame = ((JArray)jObject["Value"]["Frame"]).ToObject<List<string>>(),
                        Axis = ((JArray)jObject["Value"]["Axis"]).ToObject<List<string>>(),
                        ActionQueries = new List<ActionQueries> {
                    new ActionQueries
                    {
                        QueryType = method,
                        Name = name,
                        Value = value,
                        ValueType = valuetype,
                    }
                        }
                    });

                };
                };

            // grasshopperへ出力
            DA.SetData("Plan", plans);

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
                return test2.Properties.Resources.delete;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("06dbd869-1d9e-4434-ae04-9dfee6ffdd82"); }
        }
    }
}