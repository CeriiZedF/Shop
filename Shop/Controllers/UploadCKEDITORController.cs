using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Shop.Controllers
{
    [Authorize(Policy = WC.AdminManagerPolicy)]
    public class UploadCKEDITORController : Controller
    {
        private readonly IWebHostEnvironment _webHost;

        public UploadCKEDITORController(IWebHostEnvironment webHostEnvironment)
        {
            _webHost = webHostEnvironment;
        }

        public IActionResult Index()
        {
            return View();
        }

        // загрузка файла на наш сервер с помощью ckeditor
        [HttpPost]
        public async Task<JsonResult> Upload(IFormFile upload)
        {
            if (upload is null || upload.Length <= 0)
            {
                return Json(new { path = "/uploads/" });
            }
            string filename = upload.FileName;
            string path = Path.Combine(_webHost.WebRootPath, "uploads", filename);
            using (FileStream stream = new(path, FileMode.Create))
            {
                await upload.CopyToAsync(stream);
            }
            string url = $"/uploads/{filename}";
            return Json(new { uploaded = true, url = url });
        }

        // получаем все файлы из сервера и возвращаем их отображение
        [HttpGet]
        public IActionResult FileBrowser()
        {
            DirectoryInfo dir = new(Path.Combine(_webHost.WebRootPath, "uploads"));
            ViewBag.FilesUploads = dir.GetFiles();
            return View();
        }
    }
}
