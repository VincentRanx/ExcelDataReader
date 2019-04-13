using Devil.ContentProvider;
using Devil.Utility;
using Newtonsoft.Json.Linq;
namespace Game.ContentProvider
{	public class Constellation : TableBase
	{
		public TextRef name {get; private set;}
		public BirthDate start {get; private set;}
		public BirthDate end {get; private set;}
		public override void Init(JObject obj)
		{
			base.Init(obj);
			name = obj.Value<TextRef>("name");
			start = obj.Value<BirthDate>("start");
			end = obj.Value<BirthDate>("end");
		}
	}
}