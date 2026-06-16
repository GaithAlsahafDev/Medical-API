using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MedicalAppAPI.Controllers;

[Authorize(Roles = "Admin,Doctor")]
[ApiController]
[Route("api/[controller]")]
public class PatientsController : ControllerBase
{
    private readonly AppDbContext _context;

    public PatientsController(AppDbContext context)
    {
        _context = context;
    }

    // 1. جلب كل المرضى من قاعدة البيانات
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Patient>>> GetPatients()
    {
        return await _context.Patients.OrderByDescending(p => p.CreatedAt).ToListAsync();
    }

    // 2. إضافة مريض جديد لقاعدة البيانات
    [HttpPost]
    public async Task<ActionResult<Patient>> PostPatient([FromBody] Patient patient)
    {
        // 🟢 قراءة الأخطاء بالتفصيل إذا كانت الحقول القادمة من الفرونتند تالفة أو غير متطابقة
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        if (patient == null || string.IsNullOrEmpty(patient.Name))
        {
            return BadRequest("بيانات المريض غير صالحة");
        }

        //  جلب اسم المستخدم الحالي المشفر داخل التوكن (الدكتور أو الأدمن)
        var currentUsername = User.Identity?.Name;

        patient.Name = patient.Name.Trim();
        patient.CreatedAt = DateTime.UtcNow;
        patient.AddedBy = !string.IsNullOrEmpty(currentUsername) ? currentUsername : "غير مسجل";

        _context.Patients.Add(patient);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetPatients), new { id = patient.Id }, patient);
    }

    [HttpDelete("{id}")]
[Authorize(Roles = "Admin")] // فقط الأدمن لديه صلاحية الحذف
public async Task<IActionResult> DeletePatient(int id)
{
    var patient = await _context.Patients.FindAsync(id);
    if (patient == null)
    {
        return NotFound("المريض غير موجود.");
    }

    _context.Patients.Remove(patient);
    await _context.SaveChangesAsync();

    return Ok("تم حذف المريض بنجاح.");
}
}