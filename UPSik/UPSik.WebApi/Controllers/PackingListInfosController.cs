using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Text;
using UPSik.BusinessLayer;
using UPSik.DataLayer.Models;

namespace UPSik.WebApi.Controllers
{
    [Route("api/packing_list_infos")]
    public class PackingListInfosController
    {
        private readonly IPackingListService _packingListService;

        public PackingListInfosController(IPackingListService packingListService)
        {
            _packingListService = packingListService;
        }

        [HttpGet("{id}")]
        public List<PackingListInfo> GetPackingListInfos(int id)
        {
            var packingListInfos = _packingListService.GetCourierPackingListInfos(id);
            return packingListInfos;
        }
    }
}
