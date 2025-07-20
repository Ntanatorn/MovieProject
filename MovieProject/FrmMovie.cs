using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using System.Drawing.Imaging;


namespace MovieProject
{
    public partial class FrmMovie : Form
    {
        byte[] movieImage;
        byte[] directorImage;
         
        public FrmMovie()
        {
            InitializeComponent();
        }

        private void FrmMovie_Load(object sender, System.EventArgs e)
        {
            btSaveMovie.Enabled = true;
            btUpdateMovie.Enabled = false;
            btDeleteMovie.Enabled = false;

            getAllMovieToListView();

            cbbMovieType.SelectedIndex = 0;
        }
        private Image convertByteArrayToImage(byte[] byteArrayIn)
        {
            if (byteArrayIn == null || byteArrayIn.Length == 0)
            {
                return null;
            }
            try
            {
                using (MemoryStream ms = new MemoryStream(byteArrayIn))
                {
                    return Image.FromStream(ms);
                }
            }
            catch (ArgumentException ex)
            {
                Console.WriteLine("Error converting byte array to image: " + ex.Message);
                return null;
            }
        }
        private void getAllSearchMovie()
        {
            string keyword = tbSearchMovie.Text.Trim();

            if (string.IsNullOrEmpty(keyword))
            {
                MessageBox.Show("กรุณากรอกชื่อภาพยนตร์ที่ต้องการค้นหา", "แจ้งเตือน", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using (SqlConnection sqlConnection = new SqlConnection(ShereResource.connectionString))
            {
                try
                {
                    sqlConnection.Open();

                    string strSQL = @"SELECT movieName, movieDetail FROM movie_tb
                              WHERE movieName LIKE @keyword";

                    using (SqlCommand sqlCommand = new SqlCommand(strSQL, sqlConnection))
                    {
                        sqlCommand.Parameters.AddWithValue("@keyword", "%" + keyword + "%");

                        using (SqlDataReader reader = sqlCommand.ExecuteReader())
                        {
                            lvShowSearchMovie.Items.Clear();
                            lvShowSearchMovie.Columns.Clear();
                            lvShowSearchMovie.FullRowSelect = true;
                            lvShowSearchMovie.View = View.Details;

                            lvShowSearchMovie.Columns.Add("ชื่อภาพยนตร์", 75, HorizontalAlignment.Left);
                            lvShowSearchMovie.Columns.Add("รายละเอียด", 80, HorizontalAlignment.Left);

                            bool found = false;

                            while (reader.Read())
                            {
                                ListViewItem item = new ListViewItem(reader["movieName"].ToString());
                                item.SubItems.Add(reader["movieDetail"].ToString());

                                lvShowSearchMovie.Items.Add(item);
                                found = true;
                            }

                            if (!found)
                            {
                                MessageBox.Show("ไม่พบชื่อภาพยนตร์ที่ค้นหา", "ผลการค้นหา", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("เกิดข้อผิดพลาด: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }


        private void getAllMovieToListView()
        {
            using (SqlConnection sqlConnection = new SqlConnection(ShereResource.connectionString))
            {
                try
                {
                    sqlConnection.Open();

                    string strSQL = @"SELECT movieName, movieDate, movieType, movieImage, movieDetail
                              FROM movie_tb";

                    using (SqlDataAdapter dataAdapter = new SqlDataAdapter(strSQL, sqlConnection))
                    {
                        DataTable dataTable = new DataTable();
                        dataAdapter.Fill(dataTable);

                        lvShowAllMovie.Items.Clear();
                        lvShowAllMovie.Columns.Clear();
                        lvShowAllMovie.FullRowSelect = true;
                        lvShowAllMovie.View = View.Details;

                        if (lvShowAllMovie.SmallImageList == null)
                        {
                            lvShowAllMovie.SmallImageList = new ImageList();
                            lvShowAllMovie.SmallImageList.ImageSize = new Size(50, 50);
                            lvShowAllMovie.SmallImageList.ColorDepth = ColorDepth.Depth32Bit;
                        }
                        lvShowAllMovie.SmallImageList.Images.Clear();

                        // กำหนดคอลัมน์ตาม SELECT
                        lvShowAllMovie.Columns.Add("รูปภาพยนต์", 110, HorizontalAlignment.Left);
                        lvShowAllMovie.Columns.Add("ชื่อภาพยนต์", 150, HorizontalAlignment.Left);
                        lvShowAllMovie.Columns.Add("รายละเอียด", 125, HorizontalAlignment.Left);
                        lvShowAllMovie.Columns.Add("วันที่เข้าฉาย", 125, HorizontalAlignment.Left);
                        lvShowAllMovie.Columns.Add("ประเภทภาพยนต์", 150, HorizontalAlignment.Right);

                        foreach (DataRow dataRow in dataTable.Rows)
                        {
                            ListViewItem item = new ListViewItem();

                            Image movieImage = null;

                            if (dataRow["movieImage"] != DBNull.Value)
                            {
                                byte[] imgByte = (byte[])dataRow["movieImage"];
                                movieImage = convertByteArrayToImage(imgByte);

                                lvShowAllMovie.SmallImageList.Images.Add(movieImage);
                                item.ImageIndex = lvShowAllMovie.SmallImageList.Images.Count - 1;
                            }
                            else
                            {
                                item.ImageIndex = -1;
                            }

                            // เพิ่ม SubItems ตามลำดับคอลัมน์ที่สร้างไว้
                            item.SubItems.Add(dataRow["movieName"].ToString());
                            item.SubItems.Add(dataRow["movieDetail"].ToString());

                            if (dataRow["movieDate"] != DBNull.Value)
                            {
                                DateTime dt = Convert.ToDateTime(dataRow["movieDate"]);
                                item.SubItems.Add(dt.ToString("yyyy-MM-dd"));
                            }
                            else
                            {
                                item.SubItems.Add("");
                            }

                            item.SubItems.Add(dataRow["movieType"].ToString());

                            lvShowAllMovie.Items.Add(item);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("เกิดข้อผิดพลาด: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
        private void showWarningMSG(string msg)
        {
            MessageBox.Show(msg, "คำเตือน", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        private void btSaveMovie_Click(object sender, EventArgs e)
        {
            if (pcbMovieImage.Image != null)
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    pcbMovieImage.Image.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                    movieImage = ms.ToArray();
                }
            }

            if (pcbMovieDirectorImage.Image != null)
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    pcbMovieDirectorImage.Image.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                    directorImage = ms.ToArray();
                }
            }

            // ตรวจสอบข้อมูลที่จำเป็น
            if (movieImage == null)
            {
                showWarningMSG("เลือกรูปภาพยนต์ด้วย...");
                return;
            }
            if (string.IsNullOrWhiteSpace(tbMovieName.Text))
            {
                showWarningMSG("ป้อนชื่อภาพยนต์ด้วย...");
                return;
            }
            if (string.IsNullOrWhiteSpace(tbMovieDetail.Text))
            {
                showWarningMSG("ป้อนรายละเอียดภาพยนต์ด้วย...");
                return;
            }
            if (string.IsNullOrWhiteSpace(tbMovieDirectorName.Text))
            {
                showWarningMSG("ป้อนชื่อผู้กำกับด้วย...");
                return;
            }
            if (cbbMovieType.SelectedIndex < 0)
            {
                showWarningMSG("เลือกประเภทภาพยนต์ด้วย...");
                return;
            }

            using (SqlConnection sqlConnection = new SqlConnection(ShereResource.connectionString))
            {
                try
                {
                    sqlConnection.Open();

                    // เริ่ม Transaction
                    SqlTransaction sqlTransaction = sqlConnection.BeginTransaction();

                    string strSql = @"INSERT INTO movie_tb 
                              (movieName, movieDetail, movieDirectorName, movieDate, movieHour, movieMinute, movieType, movieImage, movieDirectorImage)
                              VALUES 
                              (@movieName, @movieDetail, @movieDirectorName, @movieDate, @movieHour, @movieMinute, @movieType, @movieImage, @movieDirectorImage)";

                    using (SqlCommand sqlCommand = new SqlCommand(strSql, sqlConnection, sqlTransaction))
                    {
                        sqlCommand.Parameters.Add("@movieName", SqlDbType.NVarChar, 150).Value = tbMovieName.Text.Trim();
                        sqlCommand.Parameters.Add("@movieDetail", SqlDbType.NVarChar, 500).Value = tbMovieDetail.Text.Trim();
                        sqlCommand.Parameters.Add("@movieDirectorName", SqlDbType.NVarChar, 150).Value = tbMovieDirectorName.Text.Trim();
                        sqlCommand.Parameters.Add("@movieDate", SqlDbType.Date).Value = dtpMovieDate.Value.Date;
                        sqlCommand.Parameters.Add("@movieHour", SqlDbType.Int).Value = (int)nudMovieHour.Value;
                        sqlCommand.Parameters.Add("@movieMinute", SqlDbType.Int).Value = (int)nudMovieMinute.Value;
                        sqlCommand.Parameters.Add("@movieType", SqlDbType.NVarChar, 150).Value = cbbMovieType.SelectedItem.ToString();
                        sqlCommand.Parameters.Add("@movieImage", SqlDbType.Image).Value = movieImage;
                        sqlCommand.Parameters.Add("@movieDirectorImage", SqlDbType.Image).Value = directorImage ?? (object)DBNull.Value;

                        sqlCommand.ExecuteNonQuery();
                    }

                    sqlTransaction.Commit();

                    MessageBox.Show("บันทึกข้อมูลภาพยนต์เรียบร้อยแล้ว", "สำเร็จ", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    // รีเฟรชข้อมูลหรือเคลียร์ฟอร์มตามต้องการ
                    getAllMovieToListView();

                    // เคลียร์ข้อมูลฟอร์ม
                    tbMovieName.Clear();
                    tbMovieDetail.Clear();
                    tbMovieDirectorName.Clear();
                    dtpMovieDate.Value = DateTime.Today;
                    nudMovieHour.Value = 0;
                    nudMovieMinute.Value = 0;
                    cbbMovieType.SelectedIndex = 0;
                    pcbMovieImage.Image = null;
                    pcbMovieDirectorImage.Image = null;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("เกิดข้อผิดพลาด: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }

        }

        private byte[] convertImageToByteArray(Image image, ImageFormat imageFormat)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                using (Bitmap bmp = new Bitmap(image))  // clone image ใหม่
                {
                    bmp.Save(ms, imageFormat);
                }
                return ms.ToArray();
            }
        }

        private byte[] movieImageBytes; // เก็บข้อมูลภาพของหนัง
        private byte[] movieDirectorImageBytes; // เก็บข้อมูลภาพผู้กำกับ

        private void btMovieImage_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.InitialDirectory = @"C:\";
            openFileDialog.Filter = "Image Files (*.jpg;*.png;)|*.jpg;*.png";

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                pcbMovieImage.Image = Image.FromFile(openFileDialog.FileName);

                if (pcbMovieImage.Image.RawFormat.Equals(ImageFormat.Jpeg))
                {
                    movieImageBytes = convertImageToByteArray(pcbMovieImage.Image, ImageFormat.Jpeg);
                }
                else
                {
                    movieImageBytes = convertImageToByteArray(pcbMovieImage.Image, ImageFormat.Png);
                }
            }
        }

        private void btMovieDirectorImage_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.InitialDirectory = @"C:\";
            openFileDialog.Filter = "Image Files (*.jpg;*.png;)|*.jpg;*.png";

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                pcbMovieDirectorImage.Image = Image.FromFile(openFileDialog.FileName);

                if (pcbMovieDirectorImage.Image.RawFormat.Equals(ImageFormat.Jpeg))
                {
                    movieDirectorImageBytes = convertImageToByteArray(pcbMovieDirectorImage.Image, ImageFormat.Jpeg);
                }
                else
                {
                    movieDirectorImageBytes = convertImageToByteArray(pcbMovieDirectorImage.Image, ImageFormat.Png);
                }
            }
        }

        private void lvShowAllMovie_ItemActivate(object sender, EventArgs e)
        {
            if (lvShowAllMovie.SelectedItems.Count > 0)
            {
                string movieName = lvShowAllMovie.SelectedItems[0].SubItems[1].Text;

                int movieId = GetMovieIdByName(movieName);
                if (movieId > 0)
                {
                    LoadMovieDataToControls(movieId);

                    btSaveMovie.Enabled = false;
                    btUpdateMovie.Enabled = true;
                    btDeleteMovie.Enabled = true;
                }
                else
                {
                    MessageBox.Show("ไม่พบข้อมูลภาพยนต์ในระบบ", "แจ้งเตือน", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
        }

        private int GetMovieIdByName(string movieName)
        {
            using (SqlConnection conn = new SqlConnection(ShereResource.connectionString))
            {
                conn.Open();
                string sql = "SELECT movieId FROM movie_tb WHERE movieName = @movieName";
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@movieName", movieName);
                    object result = cmd.ExecuteScalar();
                    if (result != null && int.TryParse(result.ToString(), out int id))
                        return id;
                }
            }
            return -1;
        }

        private void LoadMovieDataToControls(int movieId)
        {
            using (SqlConnection conn = new SqlConnection(ShereResource.connectionString))
            {
                conn.Open();
                string sql = "SELECT * FROM movie_tb WHERE movieId = @movieId";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@movieId", movieId);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            lbMovieId.Text = reader["movieId"].ToString();
                            tbMovieName.Text = reader["movieName"].ToString();
                            tbMovieDetail.Text = reader["movieDetail"].ToString();
                            tbMovieDirectorName.Text = reader["movieDirectorName"].ToString();

                            // วันที่
                            if (reader["movieDate"] != DBNull.Value)
                                dtpMovieDate.Value = Convert.ToDateTime(reader["movieDate"]);

                            // ชั่วโมง/นาที
                            if (reader["movieHour"] != DBNull.Value)
                                nudMovieHour.Value = Convert.ToDecimal(reader["movieHour"]);

                            if (reader["movieMinute"] != DBNull.Value)
                                nudMovieMinute.Value = Convert.ToDecimal(reader["movieMinute"]);

                            // ประเภทหนัง
                            string type = reader["movieType"].ToString();
                            int index = cbbMovieType.Items.IndexOf(type);
                            cbbMovieType.SelectedIndex = index >= 0 ? index : 0;

                            // รูปภาพหนัง
                            if (reader["movieImage"] != DBNull.Value)
                            {
                                byte[] img = (byte[])reader["movieImage"];
                                pcbMovieImage.Image = convertByteArrayToImage(img);
                            }
                            else
                            {
                                pcbMovieImage.Image = null;
                            }

                            // รูปภาพผู้กำกับ
                            if (reader["movieDirectorImage"] != DBNull.Value)
                            {
                                byte[] dirImg = (byte[])reader["movieDirectorImage"];
                                pcbMovieDirectorImage.Image = convertByteArrayToImage(dirImg);
                            }
                            else
                            {
                                pcbMovieDirectorImage.Image = null;
                            }
                        }
                    }
                }
            }
        }

        private void btResetMovie_Click(object sender, EventArgs e)
        {
            getAllMovieToListView();

            // เคลียร์ข้อมูลฟอร์ม
            lbMovieId.Text = "";
            tbMovieName.Clear();
            tbMovieDetail.Clear();
            tbMovieDirectorName.Clear();
            tbSearchMovie.Clear();
            dtpMovieDate.Value = DateTime.Today;
            nudMovieHour.Value = 0;
            nudMovieMinute.Value = 0;
            cbbMovieType.SelectedIndex = 0;
            pcbMovieImage.Image = null;
            pcbMovieDirectorImage.Image = null;

            // รีเซ็ตปุ่ม
            btSaveMovie.Enabled = true;
            btUpdateMovie.Enabled = false;
            btDeleteMovie.Enabled = false;

            // เคลียร์ข้อมูลใน ListView การค้นหา
            lvShowSearchMovie.Items.Clear();
            lvShowSearchMovie.Columns.Clear(); // ถ้าต้องการเคลียร์หัวคอลัมน์ด้วย
        }

        private void btExit_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btSearchMovie_Click(object sender, EventArgs e)
        {
            getAllSearchMovie();
        }

        private byte[] movieDirectorImage = null;
        private void btUpdateMovie_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(lbMovieId.Text))
            {
                MessageBox.Show("กรุณาเลือกรายการภาพยนตร์ก่อน", "แจ้งเตือน", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // ตรวจสอบข้อมูลที่จำเป็น
            if (string.IsNullOrWhiteSpace(tbMovieName.Text) ||
                string.IsNullOrWhiteSpace(tbMovieDetail.Text) ||
                string.IsNullOrWhiteSpace(tbMovieDirectorName.Text))
            {
                MessageBox.Show("กรุณากรอกข้อมูลให้ครบถ้วน", "แจ้งเตือน", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            byte[] imgMovie = null;
            byte[] imgDirector = null;

            if (pcbMovieImage.Image != null)
                imgMovie = convertImageToByteArray(pcbMovieImage.Image, ImageFormat.Jpeg);  // หรือ Png ตามที่ใช้
            else if (movieImage != null)
                imgMovie = movieImage;

            if (pcbMovieDirectorImage.Image != null)
                imgDirector = convertImageToByteArray(pcbMovieDirectorImage.Image, ImageFormat.Jpeg);
            else if (movieDirectorImage != null)
                imgDirector = movieDirectorImage;

            using (SqlConnection sqlConnection = new SqlConnection(ShereResource.connectionString))
            {
                try
                {
                    sqlConnection.Open();

                    string strSQL = @"UPDATE movie_tb SET 
                                movieName = @name,
                                movieDetail = @detail,
                                movieDirectorName = @director,
                                movieDate = @date,
                                movieHour = @hour,
                                movieMinute = @minute,
                                movieType = @type,
                                movieImage = @movieImage,
                                movieDirectorImage = @directorImage
                              WHERE movieId = @id";

                    using (SqlCommand cmd = new SqlCommand(strSQL, sqlConnection))
                    {
                        cmd.Parameters.AddWithValue("@name", tbMovieName.Text.Trim());
                        cmd.Parameters.AddWithValue("@detail", tbMovieDetail.Text.Trim());
                        cmd.Parameters.AddWithValue("@director", tbMovieDirectorName.Text.Trim());
                        cmd.Parameters.AddWithValue("@date", dtpMovieDate.Value.Date);
                        cmd.Parameters.AddWithValue("@hour", (int)nudMovieHour.Value);
                        cmd.Parameters.AddWithValue("@minute", (int)nudMovieMinute.Value);
                        cmd.Parameters.AddWithValue("@type", cbbMovieType.SelectedItem?.ToString() ?? "");
                        cmd.Parameters.AddWithValue("@movieImage", (object)imgMovie ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@directorImage", (object)imgDirector ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@id", int.Parse(lbMovieId.Text));

                        cmd.ExecuteNonQuery();
                    }

                    MessageBox.Show("อัปเดตข้อมูลเรียบร้อยแล้ว", "สำเร็จ", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    getAllMovieToListView();
                    btResetMovie_Click(null, null); // รีเซ็ตหน้าจอ
                }
                catch (Exception ex)
                {
                    MessageBox.Show("เกิดข้อผิดพลาด: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }


        private void btDeleteMovie_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(lbMovieId.Text))
            {
                MessageBox.Show("กรุณาเลือกรายการที่ต้องการลบ", "แจ้งเตือน", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            DialogResult result = MessageBox.Show("คุณต้องการลบข้อมูลนี้ใช่หรือไม่?", "ยืนยันการลบ", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result != DialogResult.Yes) return;

            int movieId = int.Parse(lbMovieId.Text);

            using (SqlConnection conn = new SqlConnection(ShereResource.connectionString))
            {
                try
                {
                    conn.Open();
                    string query = "DELETE FROM movie_tb WHERE movieId = @movieId";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@movieId", movieId);
                        cmd.ExecuteNonQuery();

                        MessageBox.Show("ลบข้อมูลเรียบร้อยแล้ว", "สำเร็จ", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        btResetMovie.PerformClick(); // รีเซ็ตฟอร์มและรีโหลดข้อมูล
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("เกิดข้อผิดพลาด: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

    }
}
