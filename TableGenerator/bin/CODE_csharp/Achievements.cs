using Devil.ContentProvider;
using Devil.Utility;
using Newtonsoft.Json.Linq;
namespace Game.ContentProvider
{	public class Achievements : TableBase
	{
		public TextRef name {get; private set;}
		public TextRef descript {get; private set;}
		public int impact_shop_id {get; private set;}
		public int impact_shop_tag {get; private set;}
		public float income_multiper {get; private set;}
		public float time_muliper {get; private set;}
		public int unlock_lv {get; private set;}
		public int shop_mask {get; private set;}
		public int feedback {get; private set;}
		public override void Init(JObject obj)
		{
			base.Init(obj);
			name = obj.Value<TextRef>("name");
			descript = obj.Value<TextRef>("descript");
			impact_shop_id = obj.Value<int>("impact_shop_id");
			impact_shop_tag = obj.Value<int>("impact_shop_tag");
			income_multiper = obj.Value<float>("income_multiper");
			time_muliper = obj.Value<float>("time_muliper");
			unlock_lv = obj.Value<int>("unlock_lv");
			shop_mask = obj.Value<int>("shop_mask");
			feedback = obj.Value<int>("feedback");
		}
	}
}