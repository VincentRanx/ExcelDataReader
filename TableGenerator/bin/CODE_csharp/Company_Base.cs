using Devil.ContentProvider;
using Devil.Utility;
using Newtonsoft.Json.Linq;
namespace Game.ContentProvider
{	public class Company_Base : TableBase
	{
		public TextRef name {get; private set;}
		public int tag {get; private set;}
		public string icon {get; private set;}
		public override void Init(JObject obj)
		{
			base.Init(obj);
			name = obj.Value<TextRef>("name");
			tag = obj.Value<int>("tag");
			icon = obj.Value<string>("icon");
		}
	}
}