using qyn_figure.Models;
using qyn_figure.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace qyn_figure.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class ContactInfoController : Controller
    {
        private readonly QynFigureContext _context;
        private readonly IWebHostEnvironment _webHostEn;
        public ContactInfoController(QynFigureContext context, IWebHostEnvironment webHostEn)
        {
            _context = context;
            _webHostEn = webHostEn;
        }

        //Phân trang cho Index
        public async Task<IActionResult> Index(int pg = 1)
        {
            List<ContactInfo> contacts = _context.ContactInfos.ToList();


            const int pageSize = 10; //10 items/trang

            if (pg < 1) //page < 1;
            {
                pg = 1; //page ==1
            }
            int recsCount = contacts.Count(); //33 items;

            var pager = new Paginate(recsCount, pg, pageSize);

            int recSkip = (pg - 1) * pageSize; //(3 - 1) * 10; 

            var data = contacts.Skip(recSkip).Take(pager.PageSize).ToList();

            ViewBag.Pager = pager;

            return View(data);
        }

        [HttpGet]
        public IActionResult Add()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(ContactInfo contact)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Xử lý upload ảnh
                    if (contact.ImageUpload != null && contact.ImageUpload.Length > 0)
                    {
                        string uploadsDir = Path.Combine(_webHostEn.WebRootPath, "img/info_img");

                        // Tạo thư mục nếu chưa tồn tại
                        if (!Directory.Exists(uploadsDir))
                        {
                            Directory.CreateDirectory(uploadsDir);
                        }

                        string uniqueFileName = Guid.NewGuid().ToString() + "_" + contact.ImageUpload.FileName;
                        string filePath = Path.Combine(uploadsDir, uniqueFileName);

                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await contact.ImageUpload.CopyToAsync(fileStream);
                        }

                        contact.LogoPath = "img/info_img/" + uniqueFileName;
                    }

                    _context.ContactInfos.Add(contact);
                    await _context.SaveChangesAsync();

                    TempData["success"] = "Thêm thông tin thành công";
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    // Log lỗi
                    Console.WriteLine(ex.Message);
                    TempData["error"] = "Có lỗi xảy ra: " + ex.Message;
                    return View(contact);
                }
            }

            // Hiển thị lỗi validation
            var errors = ModelState.Values.SelectMany(v => v.Errors);
            foreach (var error in errors)
            {
                TempData["error"] += error.ErrorMessage + "<br>";
            }

            return View(contact);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int Id)
        {           
            var contact = await _context.ContactInfos.FindAsync(Id);
            return View(contact);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int Id, ContactInfo contact)
        {
            
            if (Id != contact.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var updateContact = await _context.ContactInfos.FindAsync(Id);

                    if (contact.ImageUpload != null)
                    {
                        string upLoadDir = Path.Combine(_webHostEn.WebRootPath, "img/info_img");
                        string imgName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(contact.ImageUpload.FileName);
                        string filePath = Path.Combine(upLoadDir, imgName);

                        using (var fs = new FileStream(filePath, FileMode.Create))
                        {
                            await contact.ImageUpload.CopyToAsync(fs);
                        }
                        updateContact.LogoPath = "img/info_img/" + imgName;
                    }

                    updateContact.CompanyName = contact.CompanyName;
                    updateContact.Address = contact.Address;
                    updateContact.PhoneNumber = contact.PhoneNumber;
                    updateContact.Email = contact.Email;
                    updateContact.FacebookLink = contact.FacebookLink;
                    updateContact.ZaloLink = contact.ZaloLink;
                    updateContact.YoutubeLink = contact.YoutubeLink;
                    updateContact.TwitterLink = contact.TwitterLink;
                    updateContact.MapEmbedCode = contact.MapEmbedCode;
                    updateContact.WorkingHours = contact.WorkingHours;
                    updateContact.ShortDescription = contact.ShortDescription;
                    
                    _context.ContactInfos.Update(updateContact);
                    await _context.SaveChangesAsync();
                    TempData["success"] = "Cập nhật thông tin thành công";
                    return RedirectToAction("Index");
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ContactExists(contact.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }
            return View(contact);
        }

        private bool ContactExists(int id)
        {
            return _context.ContactInfos.Any(e => e.Id == id);
        }

        public async Task<IActionResult> Remove(int Id)
        {
            ContactInfo contact = await _context.ContactInfos.FindAsync(Id);
            _context.ContactInfos.Remove(contact);
            _context.SaveChanges();
            TempData["error"] = "Xóa thông tin thành công";
            return RedirectToAction("Index");
        }
    }
}
