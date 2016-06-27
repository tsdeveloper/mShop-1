﻿using mShop.Constants;
using mShop.Extensions;
using mShop.Models;
using mShop.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mShop.Presenters
{
    class ShopControlPresenter : IPresenter
    {
        public event EventHandler<ViewChangedArgs> ViewChanged;
        private ShopControlView _view;
        private Model _model;
        private ShoppingCart _cart = new ShoppingCart();

        public ShopControlPresenter(Model model, ShopControlView view)
        {
            _view = view;
            _model = model;
            _model.OpenConnection(ConnectionType.Shop);
            _view.ForceUpdateProductsList += UpdateProductsList;
            _view.SearchProduct += View_SearchProduct;
            _view.Logout += View_Logout;
            _view.ProductChecked += View_ProductChecked;
            _view.LoginOfCurrentUser = _model.Login;
        }

        private void View_ProductChecked(products_in_shop item, int quantity)
        {
            if(quantity > 0 && quantity <= item.Quantity)
            { 
                _cart.AddProduct(item, quantity);
            }
            _view.UpdateCart(_cart.GetProducts());
        }

        private void View_SearchProduct(object sender, SearchItemArgs e)
        {
            List<products_in_shop> data = null;
            switch (e.Type)
            {
                case SearchItemType.Name:
                    data = _model.ShopModel.GetProductsByName(e.Value);
                    break;
                case SearchItemType.Brand:
                    data = _model.ShopModel.GetProductsByBrand(e.Value);
                    break;
                case SearchItemType.Category:
                    data = _model.ShopModel.GetProductsByCategory(ConstantTexts.Categories[e.Value]);
                    break;
                default:
                    break;
            }
            if(data.Count > 0)
            {
                _model.ShopModel.TemporaryProductsData = data;
                var dict = CompareWithCartAndReturnDictionary(data);
                _view.UpdateProductsList(dict);
            }
            else
            {
                _model.ShopModel.TemporaryProductsData = null;
                _view.UpdateProductsList(null);
                _view.SetSearchError(ConstantTexts.CannotFindProducts);
            }
        }

        private void View_Logout()
        {
            _model.Login = "";
            _model.Password = "";
            _model.CloseConnection();
            ViewChanged?.Invoke(this, new ViewChangedArgs(ViewType.Login));
        }

        public void UpdateView(List<string> data)
        {
            throw new NotImplementedException();
        }
        
        public void UpdateProductsList()
        {
            var data = CompareWithCartAndReturnDictionary(_model.ShopModel.TemporaryProductsData);
            _view.UpdateProductsList(data);
        }

        public void UpdateView(string data)
        {
            throw new NotImplementedException();
        }

        private Dictionary<products_in_shop, int> CompareWithCartAndReturnDictionary(List<products_in_shop> list)
        {
            var data = new Dictionary<products_in_shop, int>();
            foreach (var item in list)
            {
                data.Add(item, ProductQuantityInCart(item));
            }
            return data;
        }

        private bool IsInCart(products_in_shop item) => _cart.GetProducts().ContainsKey(item);
        private int ProductQuantityInCart(products_in_shop item) => _cart.ProductQuantity(item);
    }
}
