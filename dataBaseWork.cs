


using Microsoft.Data.Sqlite;
using System.Text;

/*
	* удалить из базы всю временную хрень кроме юникс тайм и микросекунд
	сделать вывод в таблице по красивее )

*/ 
/* start table
<table style="border-collapse: collapse;" border="1">

<tr>
<td style="width: 639.29px;" colspan="10">&nbsp;</td>
</tr>
<tr>
<td style="width: 3%;">&nbsp;</td>
<td>unix_time_sec</td>
<td>microsec</td>
<td>protocol</td>
<td>scrambled</td>
<td>IGMP Message</td>
<td>Check Summ Error</td>
<td>late</td>
<td>error</td>
<td style="width: 3%">&nbsp;</td>
</tr>

*/

namespace statServer
{
	enum Protocols:int
	{
		udp = 17,
		igmp = 2,
		none = 0
	}
	enum IgmpMessages:int
	{
		IGMPv2_MembershipReport = 0x16,
		IGMPv2_LeaveGroup = 0x17,
		IGMPv2_MembershipQuery = 0x11,
		none = 0
	}
	class DataItem
	{
		public int data_year = default;
		public int data_month = default;
		public int data_day = default;
		public int time_hour = default;
		public int time_min = default;
		public int time_sec = default;
		public int time_usec = default;
		public int time_unix_sec = default;
		public Protocols protocol = Protocols.none;
		public int scramled = default;
		public IgmpMessages igmpMessage = IgmpMessages.none;
		public int checkSummErr = default;
		public int late = default;
		public int error = default;
		public DataItem()
		{
		}
		//-------------------------------------------------------------------------------------------------------------------------------------
		public static string TableStart()
		{

			StringBuilder ret = new();
			ret.Append("<table style=\"border-collapse: collapse;\" border=\"1\"><tr><td colspan=\"11\">&nbsp;</td>");
			ret.Append("</tr><tr><td style=\"width: 3%;\">&nbsp;</td><td>date time</td><td> &nbsp;unix_time_sec&nbsp; </td><td> &nbsp;microsec&nbsp;</td><td> &nbsp;protocol &nbsp;</td><td>&nbsp;scrambled &nbsp;</td>");
			ret.Append("<td> &nbsp;IGMP Message&nbsp; </td><td> &nbsp;Check Summ Error&nbsp; </td><td> &nbsp;late &nbsp;</td><td> &nbsp;error&nbsp; </td><td style=\"width: 3%\">&nbsp;</td></tr>");
			return ret.ToString();
		}
		//-------------------------------------------------------------------------------------------------------------------------------------
		public string ToTableRow()
		{
			DateTime timel = DateTime.UnixEpoch.AddSeconds(time_unix_sec);
			string timeStr = timel.ToShortDateString() + " " + timel.ToShortTimeString();
			string ret = $"<tr align = \"center\" ><td></td> <td>&nbsp;&nbsp;{timeStr}&nbsp;&nbsp;</td><td>{time_unix_sec}</td><td>{time_usec}</td><td>{protocol.ToString()}</td><td>{scramled}</td><td>{igmpMessage.ToString()}</td><td>{checkSummErr}</td><td>{late}</td><td>{error}</td><td></td></tr>";
			return ret;
		}
		//--------------------------------------------------------------------------------------------------------------------------------------
		public static string TableOver()
		{
			return "</table>";
		}
		//--------------------------------------------------------------------------------------------------------------------------------------
		public override string ToString() 
		{

			return $"{time_unix_sec}.{time_usec}\t\t{protocol.ToString()}\t{scramled}\t{igmpMessage.ToString()}\t{checkSummErr}\t{late}\t{error}";
		}
	}

	class RequestActions /// сюда будет заносится параметры переданные через post форму
	{
		readonly DateTime unixZeroTime;
		public RequestActions()
		{
			unixZeroTime  =  DateTime.UnixEpoch; //  new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
			late = false;
			scrambled = false;
			checkSumm = false;
			allErrors = false;
			igmp = false;
			limit = 10;
		}
		public string? dateFrom;
		public string? dateTo;
		public bool scrambled;
		public bool late;
		public bool checkSumm;
		public bool allErrors;	
		public bool igmp;	
		public int limit;

		
		public void printAll()
		{
			string str = $"from {dateFrom} to {dateTo} scrabled: {scrambled} ceckSumm: {checkSumm} allErrors: {allErrors} ";
			System.Console.WriteLine(str);
		}
		//-------------------------------------------------------------------------------------------------------------------

		public string? getSQL()
		{
			//string? result = "";
			var sql = new StringBuilder();

			long t1,t2;
			t1 = t2 = 0;
			
			sql.Append("SELECT * FROM PacksEr2 WHERE ");
			DateTime dtFrom = new();
			DateTime dtTo = new();

			if ( !DateTime.TryParse(dateFrom, out dtFrom))
			{
				System.Console.WriteLine("CS error to parse time" + dateFrom);
				return null;
			}
			if ( !DateTime.TryParse(dateTo, out dtTo))
			{
				System.Console.WriteLine("CS error to parse time" + dateTo);
				return null;
			}	
			//if (dtFrom.Year < dtTo.Year)	

			t1 = (long)dtFrom.Subtract(unixZeroTime).TotalSeconds;
			t2 = (long)dtTo.Subtract(unixZeroTime).TotalSeconds;

			sql.Append($"(time_unix_sec BETWEEN {t1} AND {t2}) ");
			
			if (allErrors) scrambled = late = checkSumm = true;
			bool ers = scrambled || late || checkSumm;
			
			if (ers)
			{
				sql.Append("AND ( ");
				if (scrambled) 	sql.Append("scrambled > 0 ");
				if (late && scrambled) sql.Append("OR ");
				if (late) 	sql.Append("late > 0 ");
				if (checkSumm && (late || scrambled)) 	sql.Append("OR "); //"AND checkSummmErr > 0");
				if (checkSumm) 	sql.Append("checkSummErr > 0 ");
				sql.Append(") ");	
			}	

			if (igmp && !ers) sql.Append($"AND protocol = {((int)Protocols.igmp)}");
			if (igmp && ers) sql.Append($"OR protocol = {((int)Protocols.igmp)}");
			string g = sql.ToString();			
			return sql.ToString();;
		}
	}
/////
class DataBase
{
	
	public SqliteConnection? db {get; private set;}
	DataItem dataItem;
	public DataBase(string dbPath)
	{
		dataItem = new DataItem();
		string path = "Data Source="+dbPath ;
		db = new SqliteConnection(path);
		if (db == null) System.Console.WriteLine("CS data base path error!");
		try
		{
			db?.Open();
		}
		catch
		{
			System.Console.WriteLine("CS data base not opened.");			
		}
	}
	~DataBase()
	{
		db?.Close();
		db?.Dispose();
	}
	public List<DataItem> readData(string? sqlComamnd)
	{
		
		using var cmd = new SqliteCommand (sqlComamnd, db);
		//using var cmd = new SQLiteCommand("SELECT * FROM PacksEr2;", db);
		using SqliteDataReader rdr = cmd.ExecuteReader();
		List<DataItem> items = new();
		DataItem data;

		while (rdr.Read())
		{
			data = new();
			
			data.time_unix_sec = rdr.GetInt32(0);
			data.time_usec = rdr.GetInt32(1);
			data.protocol = (Protocols)rdr.GetInt32(2);
			data.scramled = rdr.GetInt32(3);
			data.igmpMessage = (IgmpMessages)rdr.GetInt32(4);
			data.checkSummErr = rdr.GetInt32(5);
			data.late = rdr.GetInt32(6);
			data.error = rdr.GetInt32(7);

    		items.Add(data);			
		}
		return items;
	}


}



}