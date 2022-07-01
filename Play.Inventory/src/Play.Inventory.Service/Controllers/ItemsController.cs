using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Play.Common;
using Play.Inventory.Service.Clients;
using Play.Inventory.Service.Entities;

namespace Play.Inventory.Service.Controllers
{
    [ApiController]
    [Route("items")]
    public class ItemsController: ControllerBase
    {
        private readonly IRepository<InventoryItem> itemRepository;

        private readonly CatalogClient catalogClient;
        public ItemsController(IRepository<InventoryItem> itemRepository, CatalogClient catalogClient){
         this.itemRepository = itemRepository;   
         this.catalogClient = catalogClient;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<InventoryItemDto>>> GetAsync(Guid userId){
            if(userId == Guid.Empty){
                return BadRequest();
            }

            var catalogItems = await catalogClient.GetCatalogItemsAsync();
            var inventoryItemEntities = await itemRepository.GetAllAsync(item => item.UserId == userId);
            
            var inventoryItemsDtos = inventoryItemEntities.Select(inventoryItem => {
                var catalogItem = catalogItems.SingleOrDefault(catalogItem => catalogItem.Id == inventoryItem.CatalogItemId);
                return inventoryItem.AsDto(catalogItem.Name, catalogItem?.Description);
            });

            return Ok(inventoryItemsDtos);
        }
        
        [HttpPost]
        public async Task<ActionResult> PostAsync(GrantItemsDto grantItemsDto)
        {
            var inventoryItem = await itemRepository.GetAsync(item => grantItemsDto.UserId == item.UserId && item.CatalogItemId == grantItemsDto.CatalogItemId);
            if(inventoryItem == null){
                inventoryItem = new InventoryItem{
                    CatalogItemId = grantItemsDto.CatalogItemId,
                    UserId = grantItemsDto.CatalogItemId,
                    Quantity = grantItemsDto.Quantity,
                    AcquiredDate = DateTimeOffset.UtcNow
                };

                await itemRepository.CreateAsync(inventoryItem);
            }else{  
                inventoryItem.Quantity += grantItemsDto.Quantity;
                await itemRepository.UpdateAsync(inventoryItem);
            }

            return Ok();
        }
    }
}