﻿using BusinessObject.DTOs;
using BusinessObject.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DataAccess.DAO
{
    public class TicketDAO
    {
        private static GreenGardenContext context = new GreenGardenContext();

        public static List<TicketDTO> GetAllTickets()
        {
            var tickets = context.Tickets
                .Include(ticket => ticket.TicketCategory)
                .Select(ticket => new TicketDTO
                {
                    TicketId = ticket.TicketId,
                    TicketName = ticket.TicketName,
                    Price = ticket.Price,
                    TicketCategoryName = ticket.TicketCategory.TicketCategoryName
                }).ToList();
            return tickets;
        }

        public static void AddTicket(AddTicket ticketDto)
        {
            var ticket = new Ticket
            {
                TicketId = ticketDto.TicketId,
                TicketName = ticketDto.TicketName,
                Price = ticketDto.Price,
                CreatedAt = DateTime.Now,
                TicketCategoryId = ticketDto.TicketCategoryId,
                ImgUrl = ticketDto.ImgUrl,
            };
            context.Tickets.Add(ticket);
            context.SaveChanges();
        }

        public static void UpdateTicket(UpdateTicket ticketDto)
        {
            var ticket = context.Tickets.FirstOrDefault(t => t.TicketId == ticketDto.TicketId);
            if (ticket == null)
            {
                throw new Exception($"Ticket with ID {ticketDto.TicketId} does not exist.");
            }

            ticket.TicketName = ticketDto.TicketName;
            ticket.Price = ticketDto.Price;
            context.SaveChanges();
        }
        public static List<TicketDTO> GetTicketsByCategoryId(int categoryId)
        {
            var tickets = context.Tickets
                .Include(ticket => ticket.TicketCategory)
                .Where(ticket => ticket.TicketCategoryId == categoryId)
                .Select(ticket => new TicketDTO
                {
                    TicketId = ticket.TicketId,
                    TicketName = ticket.TicketName,
                    Price = ticket.Price,
                    TicketCategoryName = ticket.TicketCategory.TicketCategoryName
                }).ToList();
            return tickets;
        }

        public static List<TicketCategoryDTO> GetAllTicketCategories()
        {
            var tickets = context.TicketCategories

                .Select(ticket => new TicketCategoryDTO
                {
                    TicketCategoryId = ticket.TicketCategoryId,
                    TicketCategoryName = ticket.TicketCategoryName
                    ,
                    Description = ticket.Description,
                    CreatedAt = ticket.CreatedAt,

                }).ToList();
            return tickets;
        }
        public static List<TicketDTO> GetTicketsByCategoryIdAndSort(int? categoryId, int? sortBy)
        {
            var query = context.Tickets
                .Include(ticket => ticket.TicketCategory) // Sửa tên biến từ gear thành ticket
                .AsNoTracking() // Không theo dõi thực thể để cải thiện hiệu suất
                .AsQueryable();

            // Lọc theo danh mục vé nếu có categoryId
            if (categoryId.HasValue)
            {
                query = query.Where(ticket => ticket.TicketCategoryId == categoryId.Value);
            }

            // Sắp xếp theo tiêu chí sortBy
            switch (sortBy)
            {
                case 1: // Sắp xếp theo giá từ thấp đến cao
                    query = query.OrderBy(ticket => ticket.Price);
                    break;
                case 2: // Sắp xếp theo giá từ cao đến thấp
                    query = query.OrderByDescending(ticket => ticket.Price);
                    break;
                default:
                    // Mặc định sắp xếp theo tên vé nếu không có tiêu chí sắp xếp nào
                    query = query;
                    break;
            }

            // Chọn các thuộc tính cần thiết và chuyển đổi sang DTO
            var tickets = query.Select(ticket => new TicketDTO
            {
                TicketId = ticket.TicketId, // Sửa tên thuộc tính từ GearId thành TicketId
                TicketName = ticket.TicketName,
                Price = ticket.Price,
                TicketCategoryName = ticket.TicketCategory.TicketCategoryName,
                ImgUrl = ticket.ImgUrl
            }).ToList();

            return tickets;
        }
    }
}
