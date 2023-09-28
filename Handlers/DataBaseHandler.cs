using Dapper;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace WorkLifeBalance.HandlerClasses
{
    public static class DataBaseHandler // add Backup and LoadBackup
    {
        //semaphore to ensure only one method can write to the database at once
        private static SemaphoreSlim _semaphore = new(1);
        //connection string for the db
        private static readonly string ConnectionString = "Data Source=.\\RecordedData.db;Version=3;";

        //clear all downloaded residences from db
        public static async Task ClearTotalResidences()
        {
            //wait for a time when no methods writes now to the database
            await _semaphore.WaitAsync();
            //if no one writes continue
            try 
            {
                using(SQLiteConnection connection= new SQLiteConnection(ConnectionString)) 
                {
                    //make a new connection
                    await connection.OpenAsync();
                    //write to console that it started clearing
                    Console.WriteLine("Clearing TotalResidences Table");
                    using (SQLiteTransaction transaction = connection.BeginTransaction())
                    {
                        //tries to execute, if it failled rollback database
                        try
                        {
                            string sql = "DELETE FROM TotalResidences";
                            await connection.ExecuteAsync(sql);
                            transaction.Commit();
                        }
                        catch(Exception e)
                        {
                            Console.WriteLine($"Error Clearing TotalResidences Table: {e.Message}");
                            transaction.Rollback(); 
                        }
                    }
                    //close connection
                    await connection.CloseAsync();
                }
            }
            finally 
            {
                //free up semaphore so other methods can run now
                _semaphore.Release(); 
            }
        }
        //adds a list of residences to database
        public static async Task AddResidences(List<DayData> Residences)
        {
            //wait for a time when no methods writes now to the database
            await _semaphore.WaitAsync();
            //if no one writes continue
            try
            {
                //write to console that it began writing to db
                Console.WriteLine("Writing To TotalResidences Table");
                using (SQLiteConnection connection = new SQLiteConnection(ConnectionString))
                {
                    //open connection
                    await connection.OpenAsync();

                    //run a task that loops trough every residence and writes it to table
                    await Task.Run(async() =>
                    {
                        using (SQLiteTransaction transaction = connection.BeginTransaction())
                        {
                            //write each residence , if faill, rollback
                            try
                            {
                                string sql =@"INSERT INTO TotalResidences (Url,Image,Title,Price,City,County,ResidenceTye,BuyOrRent,m2,Website)
                                            VALUES (@Url,@Image,@Title,@Price,@City,@County,@ResidenceTye,@BuyOrRent,@M2,@Website)
                                            ON CONFLICT (Url) DO UPDATE SET
                                            Image = excluded.Image,
                                            Title = excluded.Title,
                                            Price = excluded.Price
                                            WHERE excluded.Price != -1";

                                foreach (DayData res in Residences)
                                {
                                    connection.Execute(sql, res);
                                }
                                await transaction.CommitAsync();
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine($"Error writing to TotalResidences Table: {e.Message}");
                                await transaction.RollbackAsync();
                            }
                        }
                    });
                    //close connection
                    await connection.CloseAsync();
                }
            }
            finally
            {
                //free up semaphore so other methods can run now
                _semaphore.Release();
            }
        }
        //gets the total residence count from table
        public static async Task<int> GetTotalResidenceCount()
        {
            //wait for a time when no methods writes now to the database
            await _semaphore.WaitAsync();
            //if no one writes to db continue

            //Total residence count 
            int Count = 0;

            try
            {
                Console.WriteLine("Reading TotalResidences Table Count");
                using (SQLiteConnection connection = new SQLiteConnection(ConnectionString))
                {
                    //start connection
                    await connection.OpenAsync();

                    //gets total number
                    try
                    {
                        string sql = "SELECT COUNT(*) FROM TotalResidences;";
                        Count = connection.QueryFirstOrDefault<int>(sql);
                    }
                    catch(Exception e)
                    {
                        Console.WriteLine($"Error Geting Count of TotalResidences Table: {e.Message}");
                    }
                    //close connection
                    await connection.CloseAsync();
                }
            }
            finally
            {
                //release sempahore so other methods can run
                _semaphore.Release();
            }
            //returns count
            return Count;
        }
        //gets filtered residence based on passed filters
        public static async Task<List<DayData>> GetFilteredResidences( int SelectedMinPrice,int SelectedMaxPrice,string City,string County,string ResidenceType,string BuyOrRent,string OrderBy,string Order,int MaxVisibleRes)
        {
            //wait for a time when no methods writes now to the database
            await _semaphore.WaitAsync();
            //if no one writes continue

            //list of filtered residences
            List<DayData> ReturnRes = new();
            
            try
            {
                //write to console it started reading database
                Console.WriteLine("Geting Filtered Residences");
                using(SQLiteConnection connection = new SQLiteConnection(ConnectionString))
                {
                    //open connection
                    await connection.OpenAsync();
                 
                    try
                    {
                        //depending on passed arguments chose between 2 sql statements
                        string sql;
                        if (County == "All")
                        {
                            sql = @$"SELECT * FROM TotalResidences 
                                        WHERE Url NOT IN (SELECT Url FROM SeenResidences) 
                                        AND Price >= {SelectedMinPrice} 
                                        AND Price <= {SelectedMaxPrice} 
                                        AND ResidenceTye = '{ResidenceType}'    
                                        AND BuyOrRent = '{BuyOrRent}'    
                                        AND City = '{City}'    
                                        ORDER BY {OrderBy} {Order} LIMIT {MaxVisibleRes}";
                        }
                        else
                        {
                            sql = @$"SELECT * FROM TotalResidences 
                                        WHERE Url NOT IN (SELECT Url FROM SeenResidences) 
                                        AND Price >= {SelectedMinPrice} 
                                        AND Price <= {SelectedMaxPrice} 
                                        AND ResidenceTye = '{ResidenceType}'    
                                        AND BuyOrRent = '{BuyOrRent}'    
                                        AND City = '{City}'    
                                        AND County = '{County}'    
                                        ORDER BY {OrderBy} {Order} LIMIT {MaxVisibleRes}";
                        }

                        //wait for return, and pass the return to a Residence class
                        var res = await connection.QueryAsync<DayData>(sql);
                        //set the return as a list
                        ReturnRes = res.ToList();

                    }
                    catch (Exception e) 
                    {
                        Console.WriteLine($"Error Retriving Filtered Res List: {e.Message}");
                    }
                    //close connection
                    await connection.CloseAsync();
                }
            }
            finally
            {
                //release semaphore so other methods could run
                _semaphore.Release();
            }
            //return filtered res
            return ReturnRes;
        }
        //gets favorite residences
        public static async Task<List<DayData>> GetFavResidences(int Offset, int SelectedMinPrice, int SelectedMaxPrice, string City, string County, string ResidenceType, string BuyOrRent, string Order, int MaxVisibleRes)
        {
            //wait for a time when no methods writes now to the database
            await _semaphore.WaitAsync();
            //if no one writes continue

            //list of favorite residences
            List<DayData> ReturnRes = new();

            try
            {
                Console.WriteLine("Geting Filtered Fav Residences");
                using (SQLiteConnection connection = new SQLiteConnection(ConnectionString))
                {
                    //open connection
                    await connection.OpenAsync();

                    try
                    {
                        //chose between 2 sql statements depending on passsed inputs
                        string sql = "";
                        if (County == "All")
                        {
                            sql = @$"SELECT * FROM TotalResidences 
                                        WHERE Url IN (SELECT Url FROM FavResidences) 
                                        AND Price >= {SelectedMinPrice} 
                                        AND Price <= {SelectedMaxPrice} 
                                        AND ResidenceTye = '{ResidenceType}'    
                                        AND BuyOrRent = '{BuyOrRent}'    
                                        AND City = '{City}'    
                                        ORDER BY Price {Order} 
                                        LIMIT {MaxVisibleRes} OFFSET {Offset}";
                        }
                        else
                        {
                            sql = @$"SELECT * FROM TotalResidences 
                                        WHERE Url IN (SELECT Url FROM SeenResidences) 
                                        AND Price >= {SelectedMinPrice} 
                                        AND Price <= {SelectedMaxPrice} 
                                        AND ResidenceTye = '{ResidenceType}'    
                                        AND BuyOrRent = '{BuyOrRent}'    
                                        AND City = '{City}'    
                                        AND County = '{County}'    
                                        ORDER BY Price {Order}
                                        LIMIT {MaxVisibleRes} OFFSET {Offset}";
                        }
                        //pass the return to ResidenceClass
                        var res = await connection.QueryAsync<DayData>(sql);
                        //convert to list
                        ReturnRes = res.ToList();

                    }
                    catch (Exception e)
                    {
                        Console.WriteLine($"Error Retriving Filtered Res List: {e.Message}");
                    }
                    //close connection
                    await connection.CloseAsync();
                }
            }
            finally
            {
                //free up semaphore so other methods can run now
                _semaphore.Release();
            }
            //return the favorite residence list
            return ReturnRes;
        }
        //save residence list Url and OldPrice
        public static async Task SaveSeenResidenceAsync(HashSet<(string,int)> SeenRes)
        {
            //wait for a time when no methods writes now to the database
            await _semaphore.WaitAsync();
            //if no one writes continue
            try
            {
                Console.WriteLine("Writing To SeenResidences Table");
                using (SQLiteConnection connection = new SQLiteConnection(ConnectionString)) 
                {
                    //open connection
                    await connection.OpenAsync();

                    //open a new task and loop trough hashet and write to table
                    await Task.Run(async() => 
                    {
                        using(SQLiteTransaction transaction = connection.BeginTransaction()) 
                        {
                            try
                            {
                                foreach ((string,int) SeenRes in SeenRes) 
                                {
                                    string sql = @$"INSERT OR REPLACE INTO SeenResidences 
                                                    (Url,OldPrice) values ('{SeenRes.Item1}',{SeenRes.Item2})";
                                    connection.Execute(sql);
                                }
                                await transaction.CommitAsync();
                            }
                            catch (Exception e) 
                            {
                                //rollback if failed
                                Console.WriteLine($"Error writing to SeenResidence Table, DataBase: {e.Message}");
                                await transaction.RollbackAsync();
                            }
                        }
                    });
                    //close connection
                    await connection.CloseAsync();
                }
            }
            finally
            {
                //free up semaphore so others methods can run
                _semaphore.Release();
            }
        }
        //get changed residences, compare SeenResidences Old Price with the price of new downloaded residence
        public static async Task<List<(DayData, int)>> GetChangedResidences(int MaxVisibleRes)
        {
            //wait for a time when no methods writes now to the database
            await _semaphore.WaitAsync();
            //if no one writes to db continue

            //list of changed residences
            List<(DayData, int)> ReturnRes = new();
            try 
            {
                Console.WriteLine("Geting Changed Residences From Db");
                using(SQLiteConnection connection = new SQLiteConnection(ConnectionString)) 
                {
                    //open connection
                    await connection.OpenAsync();

                    try
                    {
                        using(SQLiteCommand command = connection.CreateCommand()) 
                        {
                            command.CommandText = @$"SELECT n.Url,n.Image,n.Title,n.City,n.County,n.ResidenceTye,n.Website,n.Price,s.OldPrice 
                                            FROM SeenResidences s,TotalResidences n 
                                            WHERE s.Url = n.Url 
                                            AND s.OldPrice != n.Price 
                                            AND n.Price != -1
                                            LIMIT {MaxVisibleRes}";

                            //create a new reader executing the command
                            using(SQLiteDataReader reader = command.ExecuteReader())
                            {
                                while (reader.Read()) // maybe add await if it takes to much time
                                {
                                    //creates a new tuple 
                                    (DayData, int) ChangedRes = new();
                                    //create a new residence
                                    DayData residence = new DayData();
                                    //asign residence to tuple
                                    ChangedRes.Item1 = residence;

                                    //asign readed variables from databse to the Residence Variable
                                    //ChangedRes.Item1.Url = reader.GetString(0);

                                    //byte[] image = new byte[reader.GetBytes(1,0,null,0,int.MaxValue)];
                                    //reader.GetBytes(1,0,image,0,image.Length);
                                    //ChangedRes.Item1.Image = image;

                                    //ChangedRes.Item1.Title = reader.GetString(2);
                                    //ChangedRes.Item1.City = reader.GetString(3);
                                    //ChangedRes.Item1.County = reader.GetString(4);
                                    //ChangedRes.Item1.ResidenceTye = reader.GetString(5);
                                    //ChangedRes.Item1.Website = reader.GetString(6);
                                    //ChangedRes.Item1.Price = reader.GetInt32(7);

                                    //add the old price to the tuple item2
                                    ChangedRes.Item2 = reader.GetInt32(8);

                                    //add the tuple to the return list
                                    ReturnRes.Add(ChangedRes);

                                }
                            }
                        }
                    }
                    catch(Exception e)
                    {
                        Console.WriteLine($"Error Retriving Changed Res List: {e.Message}");

                    }

                    //close connection
                    await connection.CloseAsync();
                }
            }
            finally 
            {
                //free the sempaore so other methods could run
                _semaphore.Release();
            }
            //return list of changed residences
            return ReturnRes;
        }

        //gets the counties from a city depending on the quantity of residence listings in the area
        public static async Task<List<string>> GetResidenceCountys(string city,string Table)
        {
            //wait for a time when no methods writes now to the database
            await _semaphore.WaitAsync();
            //if no one writes to the db continue

            //create a new string list
            List<string> Countys = new();
            //ads ALL so the user could select all counties from a specified city
            Countys.Add("All");

            try
            {
                Console.WriteLine("Geting Residence Countys");
                using (SQLiteConnection connection = new SQLiteConnection(ConnectionString))
                {
                    //open connection
                    await connection.OpenAsync();

                    try
                    {
                        string sql = @$"SELECT DISTINCT County FROM TotalResidences 
                                        WHERE City = '{city}' 
                                        AND Price >= 0
                                        AND Url Not IN (SELECT Url FROM {Table})
                                        ORDER BY County ASC";
                        //cast the result in a string and add it to the list
                        var res = await connection.QueryAsync<string>(sql);
                        Countys.AddRange(res.ToList());
                    }
                    catch(Exception e)
                    {
                        Console.WriteLine($"Error Retriving Res County List: {e.Message}");
                    }
                    //close connection
                    await connection.CloseAsync();
                }
            }
            finally
            {
                //free the semaphore so other methods can run
                _semaphore.Release();
            }
            //return the list
            return Countys;
        }
        //add residence to favorite
        public static async Task AddFavResidence(DayData res)
        {
            //wait for a time when no methods writes now to the database
            await _semaphore.WaitAsync();
            try
            {
                Console.WriteLine("Writing To FavResidences Table");
                using (SQLiteConnection connection = new SQLiteConnection(ConnectionString))
                {
                    //open connection
                    await connection.OpenAsync();
                    //if no one writes continue
                    await Task.Run(async () =>
                    {
                        using (SQLiteTransaction transaction = connection.BeginTransaction())
                        {
                            try
                            {
                                string sql = @"INSERT INTO FavResidences (Url)
                                             VALUES (@Url)";

                                connection.Execute(sql, res);
                                await transaction.CommitAsync();
                            }
                            catch (Exception e)
                            {
                                //rols back if failed
                                Console.WriteLine($"Error writing to FavResidences Table: {e.Message}");
                                await transaction.RollbackAsync();
                            }
                        }
                    });

                    //close connection
                    await connection.CloseAsync();
                }
            }
            finally
            {
                //free semaphore so other methods can run
                _semaphore.Release();
            }
        }
        //remove a favorite residence from favorite table
        public static async Task RemoveFavResidence(string url)
        {
            //wait for a time when no methods writes now to the database
            await _semaphore.WaitAsync();
            //if no one writes continue
            try
            {
                Console.WriteLine("Deleting From FavResidences Table");
                using (SQLiteConnection connection = new SQLiteConnection(ConnectionString))
                {
                    //open connection
                    await connection.OpenAsync();

                    await Task.Run(async () =>
                    {
                        using (SQLiteTransaction transaction = connection.BeginTransaction())
                        {
                            try
                            {
                                string sql = @$"delete from FavResidences where Url = '{url}'";
                                connection.Execute(sql);
                                await transaction.CommitAsync();
                            }
                            catch (Exception e)
                            {
                                //if failed rollback
                                Console.WriteLine($"Error deleting From FavResidence Table, DataBase: {e.Message}");
                                await transaction.RollbackAsync();
                            }
                        }
                    });
                    //close connection
                    await connection.CloseAsync();
                }
            }
            finally
            {
                //free sempahore so other methods can run
                _semaphore.Release();
            }
        }
    }
}