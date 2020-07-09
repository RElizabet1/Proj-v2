using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using Newtonsoft.Json;
using System.IO;
using NPoco.Expressions;
using System.Data.SqlClient;
using MySql.Data.MySqlClient;

namespace Countries
{
    public partial class Main_Window : Form
    {
        public SqlConnection sqlConnection = null;
        public SqlCommandBuilder sqlBuilder_Сountry = null;
        public SqlCommandBuilder sqlBuilder_Cities = null;
        public SqlCommandBuilder sqlBuilder_Regions = null;
        public SqlDataAdapter sqlDataAdapter_Сountry = null;
        public SqlDataAdapter sqlDataAdapter_Cities = null;
        public SqlDataAdapter sqlDataAdapter_Regions = null;
        public DataSet dataSet = null;

        public async void Search_Click(object sender, EventArgs e)
        {
            Input_Сountry input = new Input_Сountry
            {
                entered_Сountry = textBox1.Text
            };
            Search_Сountry search = new Search_Сountry
            {
                url = "https://restcountries.eu/rest/v2/name/" + input.entered_Сountry + "?fullText=true"
            };
            WebRequest request = WebRequest.Create(search.url);
            request.Method = "GET";
            request.ContentType = "application/x-www-urlencoded";

            try
            {
                WebResponse response = await request.GetResponseAsync();
                string answer = string.Empty;
                using (Stream s = response.GetResponseStream())
                {
                    using (StreamReader reader = new StreamReader(s))
                    {
                        answer = await reader.ReadToEndAsync();
                    }
                }
                response.Close();
                string answer_Convert = answer.Trim('[', ']');
                Input_Сountry output_Сountry = JsonConvert.DeserializeObject<Input_Сountry>(answer_Convert);
                label1.Text = output_Сountry.name;
                label2.Text = output_Сountry.alpha3Code;
                label3.Text = output_Сountry.capital;
                label4.Text = output_Сountry.area.ToString();
                label5.Text = output_Сountry.population.ToString();
                label6.Text = output_Сountry.region;           
            }
            catch
            {
                MessageBox.Show("Страна не найдена! Введите корректное название.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        public void Saving_Database()
        {
            sqlDataAdapter_Сountry = new SqlDataAdapter("SELECT * FROM [dbo].[Сountries]  ", sqlConnection);
            sqlDataAdapter_Cities = new SqlDataAdapter("SELECT * FROM [dbo].[Cities] ", sqlConnection);
            sqlDataAdapter_Regions = new SqlDataAdapter("SELECT  * FROM [dbo].[Regions]", sqlConnection);
            sqlBuilder_Сountry = new SqlCommandBuilder(sqlDataAdapter_Сountry);
            sqlBuilder_Cities = new SqlCommandBuilder(sqlDataAdapter_Cities);
            sqlBuilder_Regions = new SqlCommandBuilder(sqlDataAdapter_Regions);
            sqlBuilder_Сountry.GetInsertCommand();
            sqlBuilder_Сountry.GetUpdateCommand();
            sqlBuilder_Cities.GetInsertCommand();
            sqlBuilder_Cities.GetUpdateCommand();
            sqlBuilder_Regions.GetInsertCommand();
            sqlBuilder_Regions.GetUpdateCommand();

                //Countries
            sqlDataAdapter_Сountry.InsertCommand = new SqlCommand("Create", sqlConnection)
                {
                    CommandType = CommandType.StoredProcedure
                };
            sqlDataAdapter_Сountry.InsertCommand.Parameters.Add(new SqlParameter("@Название", SqlDbType.NChar, 30, "Название"));
            sqlDataAdapter_Сountry.InsertCommand.Parameters.Add(new SqlParameter("@Код_страны", SqlDbType.NChar, 10, "Код_страны"));
            sqlDataAdapter_Сountry.InsertCommand.Parameters.Add(new SqlParameter("@Столица", SqlDbType.NChar, 30, "Столица"));
            sqlDataAdapter_Сountry.InsertCommand.Parameters.Add(new SqlParameter("@Площадь", SqlDbType.Float, 53, "Площадь"));
            sqlDataAdapter_Сountry.InsertCommand.Parameters.Add(new SqlParameter("@Население", SqlDbType.Int, 0, "Население"));
            sqlDataAdapter_Сountry.InsertCommand.Parameters.Add(new SqlParameter("@Регион", SqlDbType.NChar, 30, "Регион"));
            SqlParameter parameter = sqlDataAdapter_Сountry.InsertCommand.Parameters.Add("@Id", SqlDbType.Int, 0, "Id");
            parameter.Direction = ParameterDirection.Output;

                //Cities
            sqlDataAdapter_Cities.InsertCommand = new SqlCommand("Create1", sqlConnection)
                {
                    CommandType = CommandType.StoredProcedure
                };
            sqlDataAdapter_Cities.InsertCommand.Parameters.Add(new SqlParameter("@Название", SqlDbType.NChar, 30, "Название"));
            SqlParameter parameter1 = sqlDataAdapter_Cities.InsertCommand.Parameters.Add("@Id", SqlDbType.Int, 0, "Id");
            parameter1.Direction = ParameterDirection.Output;

                //Regions
            sqlDataAdapter_Regions.InsertCommand = new SqlCommand("Create2", sqlConnection)
                {
                    CommandType = CommandType.StoredProcedure
                };
            sqlDataAdapter_Regions.InsertCommand.Parameters.Add(new SqlParameter("@Название", SqlDbType.NChar, 30, "Название"));
            SqlParameter parameter2 = sqlDataAdapter_Regions.InsertCommand.Parameters.Add("@Id", SqlDbType.Int, 0, "Id");
            parameter2.Direction = ParameterDirection.Output;

            dataSet = new DataSet();
            sqlDataAdapter_Сountry.Fill(dataSet, "Countries");
            sqlDataAdapter_Cities.Fill(dataSet, "Cities");
            sqlDataAdapter_Regions.Fill(dataSet, "Regions");
                
                    //Countries
            DataTable dataTableCountries = dataSet.Tables["Countries"];
            DataRow newRow = dataTableCountries.NewRow();
            newRow["Название"] = label1.Text;
            newRow["Код_страны"] = label2.Text;
            newRow["Столица"] = label3.Text;
            newRow["Площадь"] = label4.Text;
            newRow["Население"] = label5.Text;
            newRow["Регион"] = label6.Text;
            dataTableCountries.Rows.Add(newRow);
            sqlDataAdapter_Сountry.Update(dataTableCountries);
            dataTableCountries.AcceptChanges();

                    //Cities
            DataTable dataTableCities = dataSet.Tables["Cities"];
            DataRow newRow1 = dataTableCities.NewRow();
            newRow1["Название"] = label3.Text;
            dataTableCities.Rows.Add(newRow1);
            sqlDataAdapter_Cities.Update(dataTableCities);
            dataTableCities.AcceptChanges();

                    //Regions
            DataTable dataTableRegions = dataSet.Tables["Regions"];
            DataRow newRow2 = dataTableRegions.NewRow();
            newRow2["Название"] = label6.Text;
            dataTableRegions.Rows.Add(newRow2);
            sqlDataAdapter_Regions.Update(dataTableRegions);
            dataTableRegions.AcceptChanges();
        }

        public Main_Window()
        {
            InitializeComponent();
        }
        public void Form1_Load(object sender, EventArgs e)
        {
            sqlConnection = new SqlConnection(@"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=C:\Users\Nikita\source\repos\Countries\Countries\Database1.mdf;Integrated Security=True");
            sqlConnection.Open();
        }
       
        public void Conclusion_Database()
        {
            try
            {
                Database_Window newForm = new Database_Window();
                newForm.dataGridViewCountries.DataSource = dataSet.Tables["Countries"];
                newForm.dataGridViewCities.DataSource = dataSet.Tables["Cities"];
                newForm.dataGridViewRegions.DataSource = dataSet.Tables["Regions"];
                newForm.Show();
            }
            catch
            {
                MessageBox.Show("Сначала сохраните страну в базу данных", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
   
        private void Conclusion_Click(object sender, EventArgs e)
        {
           Conclusion_Database();
        }

        private void Saving_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("Сохранить в базу данных?", "Сохранение", MessageBoxButtons.YesNo);
            if (result == DialogResult.Yes)
            {
                Saving_Database();
            }
        }
    }
    public class Input_Сountry
    {
        public string entered_Сountry;
        public string name;
        public string alpha3Code;
        public string capital;
        public double area;
        public double population;
        public string region;
    }
    public class Search_Сountry
    {
        public string url;
    }
}