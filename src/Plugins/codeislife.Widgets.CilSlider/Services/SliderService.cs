﻿using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using codeislife.Widgets.CilSlider.Data.Domain;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Security;
using Nop.Core.Domain.Stores;
using Nop.Data;
using Nop.Services.Customers;

namespace codeislife.Widgets.CilSlider.Services
{
    public class SliderService : ISliderService
    {
        #region Fields
        private readonly IWorkContext _workContext;
        private readonly CatalogSettings _catalogSettings;
        private readonly ICustomerService _customerService;
        private readonly IRepository<AclRecord> _aclRepository;
        private readonly IRepository<Slider> _sliderRepository;
        private readonly IRepository<SliderItem> _sliderItemRepository;
        private readonly IRepository<StoreMapping> _storeMappingRepository;
        #endregion

        #region Ctor
        public SliderService(
            IWorkContext workContext,
            CatalogSettings catalogSettings,
            ICustomerService customerService,
            IRepository<AclRecord> aclRepository,
            IRepository<Slider> sliderRepository,
            IRepository<SliderItem> sliderItemRepository,
            IRepository<StoreMapping> storeMappingRepository)
        {
            _workContext = workContext;
            _catalogSettings = catalogSettings;
            _customerService = customerService;
            _aclRepository = aclRepository;
            _sliderRepository = sliderRepository;
            _sliderItemRepository = sliderItemRepository;
            _storeMappingRepository = storeMappingRepository;
        }


        #endregion

        #region Slider
        public async Task<IPagedList<Slider>> GetAllSlidersAsync(
            string name,
            int storeId = 0,
            int pageIndex = 0,
            int pageSize = int.MaxValue,
            bool showHidden = false,
            bool? overridePublished = null)
        {
            var query = _sliderRepository.Table;

            if (!showHidden)
                query = query.Where(s => s.Published);

            if (!string.IsNullOrWhiteSpace(name))
                query = query.Where(s => s.Name.Contains(name));

            query = query.OrderBy(p => p.DisplayOrder).ThenBy(p => p.Id);

            if ((storeId > 0 && !_catalogSettings.IgnoreStoreLimitations) || (!showHidden && !_catalogSettings.IgnoreAcl))
            {
                if (!showHidden && !_catalogSettings.IgnoreAcl)
                {
                    //ACL (access control list)
                    // TODO : Mehmet -> Burayı açıkla.
                    var allowedCustomerRolesIds = await _customerService.GetCustomerRoleIdsAsync(await _workContext.GetCurrentCustomerAsync());
                    query = from c in query
                            join acl in _aclRepository.Table
                                on new
                                {
                                    c1 = c.Id,
                                    c2 = nameof(Slider)
                                }
                                equals
                                new
                                {
                                    c1 = acl.EntityId,
                                    c2 = acl.EntityName
                                } into c_acl
                            from acl in c_acl.DefaultIfEmpty()
                            where !c.SubjectToAcl || allowedCustomerRolesIds.Contains(acl.CustomerRoleId)
                            select c;
                }

                if (storeId > 0 && !_catalogSettings.IgnoreStoreLimitations)
                {
                    //Store mapping
                    // TODO : Mehmet -> Burayı açıkla.
                    query = from c in query
                            join sm in _storeMappingRepository.Table
                               on new
                               {
                                   c1 = c.Id,
                                   c2 = nameof(Slider)
                               }
                                equals
                                new
                                {
                                    c1 = sm.EntityId,
                                    c2 = sm.EntityName
                                } into c_sm
                            from sm in c_sm.DefaultIfEmpty()
                            where !c.LimitedToStores || storeId == sm.StoreId
                            select c;
                }
            }

            return await query.ToPagedListAsync(pageIndex, pageSize);
        }

        public async Task<Slider> GetSliderByIdAsync(int sliderId)
        {
            return await _sliderRepository.GetByIdAsync(sliderId);
        }

        public async Task InsertSliderAsync(Slider slider)
        {
            await _sliderRepository.InsertAsync(slider);
        }

        public async Task UpdateSliderAsync(Slider slider)
        {
            await _sliderRepository.UpdateAsync(slider);
        }

        public async Task DeleteSliderAsync(Slider slider)
        {
            await _sliderRepository.DeleteAsync(slider);
        }
        public async Task DeleteSliderAsync(IList<Slider> sliders)
        {
            await _sliderRepository.DeleteAsync(sliders);
        }
        public async Task<IList<Slider>> GetSliderByIdsAsync(ICollection<int> sliderIds)
        {
            var query = _sliderRepository.Table;

            query = query.Where(p => sliderIds.Contains(p.Id));

            return await query.ToListAsync();
        }
        #endregion

        #region SliderItem
        public Task<IList<SliderItem>> GetAllSliderItemsBySliderIdAsync(int sliderId)
        {
            throw new System.NotImplementedException();
        }

        public Task<SliderItem> GetSliderItemByIdAsync(int sliderItemId)
        {
            throw new System.NotImplementedException();
        }

        public Task InsertSliderItemAsync(SliderItem sliderItem)
        {
            throw new System.NotImplementedException();
        }

        public Task UpdateSliderItemAsync(SliderItem sliderItem)
        {
            throw new System.NotImplementedException();
        }

        public Task DeleteSliderItemAsync(SliderItem sliderItem)
        {
            throw new System.NotImplementedException();
        }
        #endregion
    }
}