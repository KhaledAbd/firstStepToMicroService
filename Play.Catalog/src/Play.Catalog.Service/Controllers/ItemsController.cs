using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Play.Catalog.Service.Dtos;
using Play.Catalog.Service.Entities;
using Play.Common;

namespace Play.Catalog.Service.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ItemsController : ControllerBase
    {
        /// private readonly ILogger<ItemsController> _logger;
        private readonly IRepository<Item> itemsRepository;

        private static readonly List<ItemDto> items = new List<ItemDto>()
        {
            new ItemDto(Guid.NewGuid(), "potion", "Resotes asmall amoutn Hp", 5, DateTimeOffset.UtcNow),
            new ItemDto(Guid.NewGuid(), "khaled", "Resotes asmall amoutn Hp", 4, DateTimeOffset.UtcNow),
            new ItemDto(Guid.NewGuid(), "ahmed", "Resotes asmall amoutn Hp", 2, DateTimeOffset.UtcNow)
        };

        public ItemsController(//ILogger<ItemsController> logger, 
                               IRepository<Item> itemsRepository)
        {
            ///_logger = logger;
            this.itemsRepository = itemsRepository;
        }

        [HttpGet]
        public async Task<IEnumerable<ItemDto>> GetAsync()
        {
            return (await itemsRepository.GetAllAsync()).Select(item => item.AsDto());
        }

        // GET /items/{id}/
        [HttpGet("{id}")]
        public async Task<ActionResult<ItemDto>> GetByIdAsync(Guid id)
        {

            var item = await itemsRepository.GetAsync(id);

            if (item == null)
            {
                return NotFound();
            }

            return item.AsDto();
        }

        [HttpPost]
        public async Task<ActionResult<ItemDto>> CreateItem(CreateItemDto createItemDto)
        {
            var item = new Item()
            {
                Id = Guid.NewGuid(),
                Name = createItemDto.Name,
                Description = createItemDto.Description,
                Price = createItemDto.Price,
                CreatedDate = DateTimeOffset.UtcNow
            };

            await itemsRepository.CreateAsync(item);
            return CreatedAtAction(nameof(GetByIdAsync), new { id = item.Id }, item);
        }
        [HttpPut("{id}")]
        public async Task<ActionResult> Put(Guid id, UpdateItemDto updateItemDto)
        {
            var existsItem = await itemsRepository.GetAsync(id);
            if (existsItem == null)
            {
                return NotFound();
            }

            existsItem.Name = updateItemDto.Name;
            existsItem.Description = updateItemDto.Description;
            existsItem.Price = updateItemDto.Price;

            await itemsRepository.UpdateAsync(existsItem);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(Guid id)
        {

            var item = await itemsRepository.GetAsync(id);
            if (item == null)
            {
                return NotFound();
            }
            await itemsRepository.RemoveAsync(id);
            return NoContent();
        }
    }
}