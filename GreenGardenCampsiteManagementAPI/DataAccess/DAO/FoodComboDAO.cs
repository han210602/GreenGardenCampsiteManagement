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
    public class FoodComboDAO
    {
        private static GreenGardenContext context = new GreenGardenContext();
        public static List<ComboFoodDTO> getAllComboFoods()
        {
            var listProducts = new List<ComboFoodDTO>();
            try
            {
                using (var context = new GreenGardenContext())
                {
                    listProducts = context.FoodCombos.Select(f =>new ComboFoodDTO
                    {
                        ComboId = f.ComboId,
                        ComboName = f.ComboName,
                        Price = f.Price,
                        ImgUrl = f.ImgUrl,
                    }).ToList();    
                }

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            return listProducts;
        }
        public static void ChangeFoodComboStatus(int comboId)
        {
            // Tìm thiết bị dựa trên GearId
            try
            {
                var campingGear = context.FoodCombos.FirstOrDefault(g => g.ComboId == comboId);

                // Kiểm tra xem thiết bị có tồn tại không
                if (campingGear == null)
                {
                    throw new Exception($"Food and Drink with ID {comboId} does not exist.");
                }

                // Đổi trạng thái (nếu đang là true thì chuyển sang false, ngược lại)
                campingGear.Status = !campingGear.Status;

                // Lưu thay đổi vào cơ sở dữ liệu
                context.SaveChanges();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        public static List<ComboFoodDTO> getAllCustomerComboFoods()
        {
            var listProducts = new List<ComboFoodDTO>();
            try
            {
                using (var context = new GreenGardenContext())
                {
                    listProducts = context.FoodCombos.Where(s => s.Status == true).Select(f => new ComboFoodDTO
                    {
                        ComboId = f.ComboId,
                        ComboName = f.ComboName,
                        Price = f.Price,
                        ImgUrl = f.ImgUrl,
                    }).ToList();
                }

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            return listProducts;
        }

        public static ComboFoodDetailDTO getComboFoodDetail(int id)
        {
            var listProducts = new ComboFoodDetailDTO();
            try
            {
                using (var context = new GreenGardenContext())
                {
                    listProducts = context.FoodCombos.Include(s=>s.FootComboItems)
                        .ThenInclude(f=>f.Item)
                     .Select(f => new ComboFoodDetailDTO
                    {
                        ComboId = f.ComboId,
                        ComboName = f.ComboName,
                        Price = f.Price,
                        ImgUrl = f.ImgUrl,
                        Description=f.Description,
                        FootComboItems=f.FootComboItems.Select(f=>new FootComboItemDTO
                        {
                            ItemId = f.ItemId,
                            ItemName=f.Item.ItemName,
                            Quantity = f.Quantity,
                        }).ToList()
                    }).FirstOrDefault(s=>s.ComboId==id);
                }

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            return listProducts;

        }
    }
}
