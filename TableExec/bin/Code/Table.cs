using Devil.ContentProvider;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
namespace Game.ContentProvider
{	public class Table : TableBase
	{
		public string panelName {get; private set;}
		public int panelMode {get; private set;}
		public int panelProperty {get; private set;}
		public string panelAsset {get; private set;}
		public override void Init(JObject obj)
		{
			base.Init(obj);
			panelName = obj.Value<string>("panelName");
			panelMode = obj.Value<int>("panelMode");
			panelProperty = obj.Value<int>("panelProperty");
			panelAsset = obj.Value<string>("panelAsset");
		}
	}
}