using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TodoApp.DTOs;
using TodoApp.RestFullAPI.Models;

namespace TodoApp.RestFullAPI.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    [Authorize]
    public class TodoController : ControllerBase
    {
        private readonly TodoDbContext context;
        public TodoController(TodoDbContext _context)
        {
            context = _context;

        }

        [HttpGet]//api/v1/todo
        public async Task<ActionResult<IEnumerable<TodoItemDTO>>> GetTodoItemsAsync()
        {
            
            int userId = Convert.ToInt32(User.Claims.FirstOrDefault(c => c.Type == Constants.UserId).Value);
            if (userId == null)
                return NotFound();
            //sadece kullanıcının eklediği datayı getir
            var items = await context.TodoItems.Where(t=>t.AppUserId==userId).ToListAsync();
            if (!items.Any())
                return NotFound();
            var model = from q in items
                        select new TodoItemDTO
                        {
                            Title = q.Title,
                            IsDone = q.IsDone,
                            TodoItemId = q.TodoItemId
                        };

            return Ok(model);
        }
        [HttpGet("{id}")]//api/v1/todo/5
        public async Task<ActionResult<TodoItemDTO>> GetTodoItemAsync(int id)
        {
            int userId = Convert.ToInt32(User.Claims.FirstOrDefault(c => c.Type == Constants.UserId).Value);
            var item = await context.TodoItems.FindAsync(id);
            if (item == null)
                return NotFound();
            if (item.AppUserId != userId)
                return BadRequest("Yetkiniz yok!");
            var model = new TodoItemDTO
            {
                Title = item.Title,
                IsDone = item.IsDone,
                TodoItemId = item.TodoItemId
            };

            return Ok(model);
        }

        [HttpPost]//api/v1/todo
        public async Task<IActionResult> PostTodoItemAsync(PostTodoItemDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            int userId = Convert.ToInt32(User.Claims.FirstOrDefault(c => c.Type == Constants.UserId).Value);
            // todo: Hocam eklerken user id eklemeyi unutmuşuz
            var item = new TodoItem
            {
                IsDone = dto.IsDone,
                Title = dto.Title,
                AppUserId = userId
            };

            await context.TodoItems.AddAsync(item);
            await context.SaveChangesAsync();

            var returnValue = CreatedAtAction("GetTodoItem",
                                                new { id = item.TodoItemId },
                                                new TodoItemDTO
                                                {
                                                    IsDone = item.IsDone,
                                                    TodoItemId = item.TodoItemId,
                                                    Title = item.Title
                                                });
            return returnValue;

        }


        [HttpPut("{id}")]//api/v1/todo/8
        public async Task<IActionResult> PutTodoItemAsync(int id, PutTodoItemDTO dto)
        {
            int userId = Convert.ToInt32(User.Claims.FirstOrDefault(c => c.Type == Constants.UserId).Value);
            if (!ModelState.IsValid)
                return BadRequest();
            if (id != dto.TodoItemId)
                return BadRequest();
            var itemFromDb = await context.TodoItems.FindAsync(id);
            if (itemFromDb == null)
                return NotFound();
            if(itemFromDb.AppUserId != userId) return BadRequest("Yetkiniz yok!");
            itemFromDb.Title = dto.Title;
            itemFromDb.IsDone = dto.IsDone;

            await context.SaveChangesAsync();
            return NoContent();

        }
        [HttpDelete("{id}")]//api/v1/todo/8
        public async Task<IActionResult> DeleteTodoItemAsync(int id)
        {
            int userId = Convert.ToInt32(User.Claims.FirstOrDefault(c => c.Type == Constants.UserId).Value);
            var itemFromDb = await context.TodoItems.FindAsync(id);
            if (itemFromDb == null)
                return NotFound();
            if (itemFromDb.AppUserId != userId) return BadRequest("Yetkiniz Yok");
            context.TodoItems.Remove(itemFromDb);
            await context.SaveChangesAsync();
            return NoContent();
        }
    }
}
