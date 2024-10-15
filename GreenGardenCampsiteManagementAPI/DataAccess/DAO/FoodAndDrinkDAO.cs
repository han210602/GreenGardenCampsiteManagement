﻿using BusinessObject.DTOs;
using BusinessObject.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.DAO
{
    public class FoodAndDrinkDAO
    {
        private static GreenGardenContext context = new GreenGardenContext();

        public static List<FoodAndDrinkDTO> GetAllFoodAndDrink()
        {
            var items = context.FoodAndDrinks
                .Include(x => x.Category)
                .Select(item => new FoodAndDrinkDTO
                {
                    ItemId = item.ItemId,
                    ItemName = item.ItemName,
                    Price = item.Price,
                    QuantityAvailable = item.QuantityAvailable,
                    Description = item.Description,
                    CategoryName = item.Category.CategoryName, // Lấy tên từ danh mục
                    ImgUrl = item.ImgUrl
                }).ToList();

            return items;
        }


        public static void AddFoodAndDrink(AddFoodOrDrinkDTO item)
        {
            var foodAndDrink = new FoodAndDrink
            {
                ItemId = item.ItemId,
                ItemName = item.ItemName,
                Price = item.Price,
                QuantityAvailable = item.QuantityAvailable,
                CreatedAt = item.CreatedAt,
                Description = item.Description,
                ImgUrl = item.ImgUrl,
                CategoryId = item.CategoryId // Id của danh mục
            };

            context.FoodAndDrinks.Add(foodAndDrink);
            context.SaveChanges();
        }


        public static void UpdateFoodOrDrink(UpdateFoodOrDrinkDTO itemDto)
        {
            var foodAndDrink = context.FoodAndDrinks.FirstOrDefault(f => f.ItemId == itemDto.ItemId);

            if (foodAndDrink == null)
            {
                throw new Exception($"Food and Drink with ID {itemDto.ItemId} does not exist.");
            }

            foodAndDrink.ItemName = itemDto.ItemName;
            foodAndDrink.Price = itemDto.Price;
            foodAndDrink.QuantityAvailable = itemDto.QuantityAvailable;
            foodAndDrink.Description = itemDto.Description;
            foodAndDrink.CategoryId = itemDto.CategoryId; // Cập nhật danh mục
            foodAndDrink.ImgUrl = itemDto.ImgUrl;

            context.SaveChanges();
        }
        public static List<FoodAndDrinkDTO> GetFoodAndDrinksBySort(int? categoryId, int? sortBy)
        {
            var query = context.FoodAndDrinks
                .Include(food => food.Category) // Đảm bảo bao gồm thông tin về danh mục
                .AsQueryable();

            // Lọc theo danh mục nếu categoryId được cung cấp
            if (categoryId.HasValue)
            {
                query = query.Where(food => food.CategoryId == categoryId.Value);
            }

            // Kiểm tra và sắp xếp theo tiêu chí được chỉ định
            if (sortBy.HasValue)
            {
                switch (sortBy.Value)
                {
                    case 1: // Sắp xếp theo giá từ thấp đến cao
                        query = query.OrderBy(food => food.Price);
                        break;
                    case 2: // Sắp xếp theo giá từ cao đến thấp
                        query = query.OrderByDescending(food => food.Price);
                        break;
                    case 3: // Sắp xếp theo tên món ăn
                        query = query.OrderBy(food => food.CreatedAt);
                        break;
                    case 4: // Sắp xếp theo số lượng có sẵn
                        query = query.OrderByDescending(food => food.QuantityAvailable);
                        break;
                    default:
                        // Không thực hiện sắp xếp nếu sortBy không hợp lệ
                        break;
                }
            }
            else
            {
                // Nếu sortBy là null, có thể chọn sắp xếp mặc định theo ItemId
                query = query; // Hoặc thuộc tính khác mà bạn muốn sắp xếp theo
            }

            // Chọn các thuộc tính cần thiết
            var foodAndDrinks = query.Select(food => new FoodAndDrinkDTO
            {
                ItemId = food.ItemId,
                ItemName = food.ItemName,
                Price = food.Price,
                QuantityAvailable = food.QuantityAvailable,
                Description = food.Description,
                CategoryName = food.Category.CategoryName, // Lấy tên danh mục
                ImgUrl = food.ImgUrl
            }).ToList();

            return foodAndDrinks;
        }


        public static List<FoodAndDrinkCategoryDTO> GetAllFoodAndDrinkCategories()
        {
            var items = context.FoodAndDrinkCategories

                .Select(item => new FoodAndDrinkCategoryDTO
                {
                    CategoryId = item.CategoryId,
                    CategoryName = item.CategoryName,
                    Description = item.Description,
                    CreatedAt = item.CreatedAt,

                }).ToList();

            return items;
        }
    }
}
