﻿using AirwayAPI.Data;
using AirwayAPI.Models;
using AirwayAPI.Models.MasterSearchModels;

namespace AirwayAPI.Controllers.MasterSearchControllers;

public static class MS_Utils
{
    public static void InsertSearchQuery(eHelpDeskContext context, SearchInput input, string searchFor, string searchType)
    {
        context.MasterSearchQueries.Add(new MasterSearchQuery
        {
            SearchText = input.Search,
            SearchFor = searchFor,
            SearchType = searchType,
            EventId = input.ID,
            SoNo = input.SONo,
            PoNo = input.PONo,
            InvNo = input.InvNo,
            PartNo = input.PartNo,
            PartDesc = input.PartDesc,
            Company = input.Company,
            Mfg = input.Mfg,
            SearchBy = input.Uname,
            SearchDate = DateTime.Now
        });
        context.SaveChanges();
    }
}