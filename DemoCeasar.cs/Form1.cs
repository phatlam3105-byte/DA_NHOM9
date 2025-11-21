using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;


namespace DemoCeasar
{
    public partial class Form1 : Form
    {
        // ================== KHAI BÁO ==================
        // Bảng băm lưu từ điển (không phân biệt hoa thường)
        HashSet<string> TuDien = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        public Form1()
        {
            InitializeComponent();

            // Tự động nạp file dictionary.txt nếu có trong thư mục chạy
            string duongDan = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "dictionary.txt");
            if (File.Exists(duongDan))
            {
                NapTuDien(duongDan);
            }
            else
            {
                // Nếu không có file, nạp một bộ từ mặc định nhỏ để test
                NapTuDienMacDinh();
            }
        }

        // ================== HÀM XỬ LÝ ==================

        // Dịch 1 ký tự theo khóa k (dùng cho mã hóa / giải mã)
        private char DoiKyTu(char c, int k)
        {
            if (!char.IsLetter(c)) return c; // bỏ qua ký tự không phải chữ
            char baseChar = char.IsUpper(c) ? 'A' : 'a';
            return (char)((((c - baseChar) + k + 26) % 26) + baseChar);
        }

        // Giải mã 1 chuỗi theo khóa k (k từ 0..25)
        private string GiaiMaTheoK(string chuoiMa, int k)
        {
            int khoaNguoc = (26 - (k % 26)) % 26; // dịch ngược
            StringBuilder kq = new StringBuilder();
            foreach (char ch in chuoiMa)
                kq.Append(DoiKyTu(ch, khoaNguoc));
            return kq.ToString();
        }

        // ================== TỪ ĐIỂN ==================

        // Nạp từ điển từ file: chuẩn hoá (chỉ lấy chữ, viết thường)
        private void NapTuDien(string duongDan)
        {
            TuDien.Clear();
            foreach (string dong in File.ReadAllLines(duongDan))
            {
                string clean = new string(dong.Where(char.IsLetter).ToArray()).ToLower();
                if (clean.Length > 0) TuDien.Add(clean);
            }
            lblBest.Text = $"Đã nạp {TuDien.Count} từ";
        }

        // Nạp một số từ mặc định (nếu không có file)
        private void NapTuDienMacDinh()
        {
            TuDien.Clear();
            string[] macDinh = new string[] { "meet", "later", "hello", "world", "test", "good", "morning" };
            foreach (var w in macDinh) TuDien.Add(w);
            lblBest.Text = $"Đã nạp mặc định {TuDien.Count} từ";
        }

        // ================== KIỂM TRA TỪ HỢP LỆ ==================

        // Đếm bao nhiêu từ của chuỗi nằm trong từ điển
        private int DemTuHopLe(string chuoi)
        {
            if (TuDien.Count == 0) return 0;

            string[] tokens = chuoi
                .ToLower()
                .Split(new char[] { ' ', ',', '.', '?', '!', '\r', '\n', '\t' }, StringSplitOptions.RemoveEmptyEntries);

            int dem = 0;
            foreach (var t in tokens)
            {
                string clean = new string(t.Where(char.IsLetter).ToArray());
                if (clean.Length > 0 && TuDien.Contains(clean))
                    dem++;
            }
            return dem;
        }

        // Đếm số nguyên âm (a,e,i,o,u) 
        private int DemNguyenAm(string s)
        {
            if (string.IsNullOrEmpty(s)) return 0;
            int dem = 0;
            foreach (char ch in s.ToLower())
                if ("aeiou".Contains(ch)) dem++;
            return dem;
        }

        
        private void btnBrute_Click(object sender, EventArgs e)
        {
            string chuoiMa = (txtInput.Text ?? "").Trim();
            if (chuoiMa == "")
            {
                MessageBox.Show("Vui lòng nhập văn bản mã hóa!");
                return;
            }

            lstResults.Items.Clear();
            var danhSach = new List<(int Khoa, string Giai, int Diem)>();

            // Thử tất cả khóa 0..25
            for (int k = 0; k < 26; k++)
            {
                string giai = GiaiMaTheoK(chuoiMa, k);
                int diem = DemTuHopLe(giai);
                danhSach.Add((k, giai, diem));
               
                lstResults.Items.Add($"Khóa {k,2}: {giai}");
            }

            // Tìm điểm lớn nhất 
            int diemMax = danhSach.Max(x => x.Diem);
            (int Khoa, string Giai, int Diem) totNhat;

            if (diemMax > 0)
            {
                // Chọn các candidate có diemMax, nếu nhiều thì chọn theo nguyên âm 
                var nhungTot = danhSach.Where(x => x.Diem == diemMax).ToList();
                totNhat = nhungTot.OrderByDescending(x => DemNguyenAm(x.Giai)).First();
            }
            else
            {
                // Nếu tất cả diem = 0 thì chọn chuỗi có nhiều nguyên âm nhất (heuristic)
                totNhat = danhSach.OrderByDescending(x => DemNguyenAm(x.Giai)).First();
            }

            // Tìm vị trí (index) của kết quả tốt nhất trong danhSach rồi chọn ListBox
            int index = danhSach.FindIndex(x => x.Khoa == totNhat.Khoa);
            if (index >= 0 && index < lstResults.Items.Count) lstResults.SelectedIndex = index;

            lblBest.Text = $"Kết quả tốt nhất: Khóa {totNhat.Khoa}";
            txtOutput.Text = totNhat.Giai;
        }

        // ================== NÚT NẠP TỪ ĐIỂN (qua giao diện) ==================
        
        private void btnNapTuDien_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Tập tin văn bản (*.txt)|*.txt";
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                NapTuDien(ofd.FileName);
                MessageBox.Show("Đã nạp từ điển thành công!");
            }
        }
        // ================== NÚT LƯU TỪ ĐIỂN (qua giao diện) ==================
        private void btnLuu_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Text File|*.txt";
            saveFileDialog.Title = "Lưu kết quả";
            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                using (StreamWriter sw = new StreamWriter(saveFileDialog.FileName))
                {
                    foreach (var item in lstResults.Items)
                    {
                        sw.WriteLine(item.ToString());
                    }
                }
                MessageBox.Show("Đã lưu kết quả thành công!");
            }
        }
        // ================== NÚT XÓA ==========================================
        private void btnXoa_Click(object sender, EventArgs e)
        {
            lstResults.Items.Clear();
        }
        // ================== NÚT HƯỚNG DẪN SỬ DỤNG TỪ ĐIỂN  ===================
        private void btnHuongDan_Click(object sender, EventArgs e)
        {
            MessageBox.Show("1. Tải từ điển: chọn file chứa danh sách từ.\n2. Brute-force: thử tất cả khả năng.\n3. Kết quả hiển thị trong ô.", "Hướng dẫn sử dụng");
        }
        // ================== NÚT THOÁT ỨNG DỤNG ===============================
        private void btnThoat_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("Bạn có chắc chắn muốn thoát ứng dụng không?", "Xác nhận thoát", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.Yes)
            {
                Application.Exit();
            }
        }
    }
}
