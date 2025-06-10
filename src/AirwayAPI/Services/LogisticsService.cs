using AirwayAPI.Data;
using AirwayAPI.Models;
using AirwayAPI.Models.FreightSheetModels;
using AirwayAPI.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AirwayAPI.Services
{
    /// <summary>
    /// Encapsulates all Freight_Sheet.asp logic (Save / Update / AddRow / Get).
    /// </summary>
    public class LogisticsService : ILogisticsService
    {
        private readonly eHelpDeskContext _context;
        // If you have an email‐sending abstraction, inject it here:
        // private readonly IEmailSender _emailSender;

        public LogisticsService(
            eHelpDeskContext context
        // IEmailSender emailSender
        )
        {
            _context = context;
            // _emailSender = emailSender;
        }

        public async Task<int> CreateFreightQuoteAsync(FreightQuoteDto dto, string currentUserName)
        {
            // 1) Look up EventID via EquipmentRequest WHERE SalesOrderNum LIKE '%{dto.Sonum}%'
            var matchingReq = await _context.EquipmentRequests
                .Where(r => r.SalesOrderNum != null && r.SalesOrderNum.Contains(dto.Sonum))
                .OrderByDescending(r => r.EventId)
                .FirstOrDefaultAsync();

            int? eventId = matchingReq?.EventId;

            // 2) If EventID found, retrieve CustomerPO from qtSalesOrder WHERE EventID = eventId
            string? customerPo = null;
            if (eventId.HasValue)
            {
                var so = await _context.QtSalesOrders
                    .FirstOrDefaultAsync(s => s.EventId == eventId.Value);
                customerPo = so?.CustomerPo;
            }

            // 3) Create new FreightQuote entity
            var newQuote = new FreightQuote
            {
                EventId = eventId,
                ShipFrom = dto.ShipFrom,
                ShipFromNum = dto.ShipFromNum,
                ShipFromAddress1 = dto.ShipFromAddress1,
                ShipFromAddress2 = dto.ShipFromAddress2,
                ShipFromAddress3 = dto.ShipFromAddress3,
                ShipFromAddress4 = dto.ShipFromAddress4,
                ShipTo = dto.ShipTo,
                ShipToNum = dto.ShipToNum,
                ShipToAddress1 = dto.ShipToAddress1,
                ShipToAddress2 = dto.ShipToAddress2,
                ShipToAddress3 = dto.ShipToAddress3,
                ShipToAddress4 = dto.ShipToAddress4,

                // ASP code set Priced = 1 and FreightSheet = 1 right away:
                Priced = true,
                FreightSheet = true,

                ShipRep = dto.SalesRep,
                ShipmentValue = dto.ShipmentValue,
                CarrierUsed = dto.CarrierUsed,
                ServiceUsed = dto.ServiceUsed,
                AirwayPo = dto.Sonum,
                ShipDate = dto.ShipDate,
                TrackNum = dto.TrackNum,
                TotalPieces = dto.TotalPieces,
                ActualWeight = dto.TotalWeight,
                ShipmentNotes = dto.ShipmentNotes,
                BillOfLading = dto.BillOfLading,

                EnteredBy = currentUserName,
                ModifiedBy = currentUserName,
                EntryDate = DateTime.UtcNow,
                ModifiedDate = DateTime.UtcNow
            };

            _context.FreightQuotes.Add(newQuote);
            await _context.SaveChangesAsync();
            int freightQuoteId = newQuote.Id;

            // 4) Insert one default FreightSo row (all zeros except PackageHandling=25, TotalFreight=25)
            var defaultLine = new FreightSo
            {
                FreightQuoteId = freightQuoteId,
                Sonum = int.TryParse(dto.Sonum, out var soNum) ? soNum : 0,
                FreightCharge = 0m,
                Markup = 0m,
                PackageHandling = 25m,
                TotalFreight = 25m,
                EnteredBy = currentUserName,
                ModifiedBy = currentUserName,
                EntryDate = DateTime.UtcNow,
                ModifiedDate = DateTime.UtcNow
            };
            _context.FreightSos.Add(defaultLine);

            // 5) Mark the FreightQuote as Priced = true (already set), so just Save
            await _context.SaveChangesAsync();

            // 6) (Optional) Send “Sales Order has Shipped” e‐mail to SalesRep:
            // var salesRepEmail = $"{dto.SalesRep}@airway.com";
            // var subject = $"Sales Order {dto.Sonum} has Shipped";
            // var bodyHtml = BuildFreightShippedEmailBody(dto, customerPo);
            // await _emailSender.SendEmailAsync(salesRepEmail, subject, bodyHtml, isHtml: true);

            return freightQuoteId;
        }

        public async Task UpdateFreightQuoteAsync(FreightQuoteDto dto, string currentUserName)
        {
            if (dto.FreightQuoteId <= 0)
                throw new ArgumentException("FreightQuoteId must be > 0 when updating.");

            // 1) Load existing FreightQuote
            var existingQuote = await _context.FreightQuotes
                .FirstOrDefaultAsync(fq => fq.Id == dto.FreightQuoteId) ?? throw new InvalidOperationException($"FreightQuote with Id={dto.FreightQuoteId} not found.");

            // 2) Update header fields
            existingQuote.ShipFrom = dto.ShipFrom;
            existingQuote.ShipFromNum = dto.ShipFromNum;
            existingQuote.ShipFromAddress1 = dto.ShipFromAddress1;
            existingQuote.ShipFromAddress2 = dto.ShipFromAddress2;
            existingQuote.ShipFromAddress3 = dto.ShipFromAddress3;
            existingQuote.ShipFromAddress4 = dto.ShipFromAddress4;
            existingQuote.ShipTo = dto.ShipTo;
            existingQuote.ShipToNum = dto.ShipToNum;
            existingQuote.ShipToAddress1 = dto.ShipToAddress1;
            existingQuote.ShipToAddress2 = dto.ShipToAddress2;
            existingQuote.ShipToAddress3 = dto.ShipToAddress3;
            existingQuote.ShipToAddress4 = dto.ShipToAddress4;
            existingQuote.ShipmentValue = dto.ShipmentValue;
            existingQuote.ServiceUsed = dto.ServiceUsed;
            existingQuote.CarrierUsed = dto.CarrierUsed;
            existingQuote.ShipRep = dto.SalesRep;
            existingQuote.TrackNum = dto.TrackNum;
            existingQuote.ShipDate = dto.ShipDate;
            existingQuote.AirwayPo = dto.Sonum;
            existingQuote.TotalPieces = dto.TotalPieces;
            existingQuote.ActualWeight = dto.TotalWeight;
            existingQuote.ShipmentNotes = dto.ShipmentNotes;
            existingQuote.ModifiedBy = currentUserName;
            existingQuote.ModifiedDate = DateTime.UtcNow;

            // 3) Upsert each FreightSo line in dto.Lines
            foreach (var lineDto in dto.Lines)
            {
                if (lineDto.Id > 0)
                {
                    // Attempt to find an existing row
                    var existingLine = await _context.FreightSos
                        .FirstOrDefaultAsync(so => so.Id == lineDto.Id && so.FreightQuoteId == dto.FreightQuoteId);

                    if (existingLine != null)
                    {
                        existingLine.Sonum = lineDto.Sonum;
                        existingLine.FreightCharge = lineDto.FreightCharge;
                        existingLine.Markup = lineDto.Markup;
                        existingLine.PackageHandling = lineDto.PackageHandling;
                        existingLine.TotalFreight = lineDto.TotalFreight;
                        existingLine.ModifiedBy = currentUserName;
                        existingLine.ModifiedDate = DateTime.UtcNow;
                    }
                    else
                    {
                        // If rowcount was zero, insert a new one
                        var newLine = new FreightSo
                        {
                            FreightQuoteId = dto.FreightQuoteId,
                            Sonum = lineDto.Sonum,
                            FreightCharge = lineDto.FreightCharge,
                            Markup = lineDto.Markup,
                            PackageHandling = lineDto.PackageHandling,
                            TotalFreight = lineDto.TotalFreight,
                            EnteredBy = currentUserName,
                            ModifiedBy = currentUserName,
                            EntryDate = DateTime.UtcNow,
                            ModifiedDate = DateTime.UtcNow
                        };
                        _context.FreightSos.Add(newLine);
                    }
                }
                else
                {
                    // Id == 0: Insert brand‐new row
                    var newLine = new FreightSo
                    {
                        FreightQuoteId = dto.FreightQuoteId,
                        Sonum = lineDto.Sonum,
                        FreightCharge = lineDto.FreightCharge,
                        Markup = lineDto.Markup,
                        PackageHandling = lineDto.PackageHandling,
                        TotalFreight = lineDto.TotalFreight,
                        EnteredBy = currentUserName,
                        ModifiedBy = currentUserName,
                        EntryDate = DateTime.UtcNow,
                        ModifiedDate = DateTime.UtcNow
                    };
                    _context.FreightSos.Add(newLine);
                }
            }

            // 4) Save everything in one shot
            await _context.SaveChangesAsync();
        }

        public async Task<int> AddFreightSoLineAsync(int freightQuoteId, string currentUserName)
        {
            // Insert a blank/new FreightSo row (everything zeroed out)
            var newLine = new FreightSo
            {
                FreightQuoteId = freightQuoteId,
                Sonum = 0,
                FreightCharge = 0m,
                Markup = 0m,
                PackageHandling = 0m,
                TotalFreight = 0m,
                EnteredBy = currentUserName,
                ModifiedBy = currentUserName,
                EntryDate = DateTime.UtcNow,
                ModifiedDate = DateTime.UtcNow
            };

            _context.FreightSos.Add(newLine);
            await _context.SaveChangesAsync();
            return newLine.Id;
        }

        public async Task<FreightQuote> GetFreightQuoteByIdAsync(int freightQuoteId)
        {
            var quote = await _context.FreightQuotes
                .AsNoTracking()
                .FirstOrDefaultAsync(fq => fq.Id == freightQuoteId);

            if (quote == null)
                throw new InvalidOperationException($"FreightQuote with Id={freightQuoteId} not found.");

            return quote;
        }

        public async Task<List<FreightSo>> GetFreightSoLinesByQuoteIdAsync(int freightQuoteId)
        {
            return await _context.FreightSos
                .AsNoTracking()
                .Where(so => so.FreightQuoteId == freightQuoteId)
                .OrderBy(so => so.Id)
                .ToListAsync();
        }

        // Optional: replicate the ASP HTML‐email body here if you need it.
        // private string BuildFreightShippedEmailBody(FreightQuoteDto dto, string? customerPo)
        // {
        //     // Build identical HTML as Freight_Sheet.asp did (omitted for brevity)
        // }
    }
}