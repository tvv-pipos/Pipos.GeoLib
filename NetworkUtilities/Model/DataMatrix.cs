namespace Pipos.Common.NetworkUtilities.Model;

public class DataMatrix
{
	private Dictionary<int, Dictionary<string, float>> _dict { set; get;}
	
	public DataMatrix(Dictionary<int, Dictionary<string, float>> dict)
	{
		_dict = dict;
	}

	public float this[int piposId, string column]
	{
		get {
			if(_dict.ContainsKey(piposId))
			{
				var d = _dict[piposId];
				if(d.ContainsKey(column))
				{
					return d[column];
				}
				else
				{
					return 0.0f;
				}
			}
			return 0.0f;
		}
	}
}