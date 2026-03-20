/**
 * Going API Documentation - app.js
 * Handles: sidebar nav injection, search/filter, scroll spy, back-to-top
 */

// ─── Sidebar Navigation Data ────────────────────────────────────────
const NAV = {
  basis: {
    label: 'Going.Basis',
    pkg: 'Going.Basis',
    items: [
      { label: 'Overview', href: '../basis/index.html', type: 'page' },
      {
        label: 'Communications',
        children: [
          { label: 'CNet', href: '../basis/communications-ls.html#class-CNet', type: 'class' },
          { label: 'CNetValue', href: '../basis/communications-ls.html#class-CNetValue', type: 'class' },
          { label: 'MC', href: '../basis/communications-mitsubishi.html#class-MC', type: 'class' },
          { label: 'ModbusRTUMaster', href: '../basis/communications-modbus.html#class-ModbusRTUMaster', type: 'class' },
          { label: 'ModbusRTUSlave', href: '../basis/communications-modbus.html#class-ModbusRTUSlave', type: 'class' },
          { label: 'ModbusTCPMaster', href: '../basis/communications-modbus.html#class-ModbusTCPMaster', type: 'class' },
          { label: 'ModbusTCPSlave', href: '../basis/communications-modbus.html#class-ModbusTCPSlave', type: 'class' },
          { label: 'MasterRTU', href: '../basis/communications-modbus.html#class-MasterRTU', type: 'class' },
          { label: 'SlaveRTU', href: '../basis/communications-modbus.html#class-SlaveRTU', type: 'class' },
          { label: 'MasterTCP', href: '../basis/communications-modbus.html#class-MasterTCP', type: 'class' },
          { label: 'SlaveTCP', href: '../basis/communications-modbus.html#class-SlaveTCP', type: 'class' },
          { label: 'MQClient', href: '../basis/communications-mqtt.html#class-MQClient', type: 'class' },
          { label: 'ModbusCRC', href: '../basis/communications-modbus.html#class-ModbusCRC', type: 'static' },
          { label: 'ModbusFunction', href: '../basis/communications-modbus.html#enum-ModbusFunction', type: 'enum' },
        ]
      },
      {
        label: 'Datas',
        children: [
          { label: 'INI', href: '../basis/datas.html#class-INI', type: 'class' },
          { label: 'Seriealize', href: '../basis/datas.html#class-Seriealize', type: 'static' },
          { label: 'BitMemory', href: '../basis/datas.html#class-BitMemory', type: 'class' },
          { label: 'WordMemory', href: '../basis/datas.html#class-WordMemory', type: 'class' },
          { label: 'WordRef', href: '../basis/datas.html#class-WordRef', type: 'class' },
          { label: 'BitAccessor', href: '../basis/datas.html#class-BitAccessor', type: 'class' },
          { label: 'IBitMemory', href: '../basis/datas.html#class-IBitMemory', type: 'interface' },
          { label: 'IWordMemory', href: '../basis/datas.html#class-IWordMemory', type: 'interface' },
        ]
      },
      {
        label: 'Extensions',
        children: [
          { label: 'Bits', href: '../basis/extensions.html#class-Bits', type: 'static' },
        ]
      },
      {
        label: 'Measure',
        children: [
          { label: 'Chattering', href: '../basis/measure.html#class-Chattering', type: 'class' },
          { label: 'Stable', href: '../basis/measure.html#class-Stable', type: 'class' },
          { label: 'ChatteringMode', href: '../basis/measure.html#enum-ChatteringMode', type: 'enum' },
          { label: 'StableMode', href: '../basis/measure.html#enum-StableMode', type: 'enum' },
        ]
      },
      {
        label: 'Tools',
        children: [
          { label: 'CryptoTool', href: '../basis/tools.html#class-CryptoTool', type: 'static' },
          { label: 'MathTool', href: '../basis/tools.html#class-MathTool', type: 'static' },
          { label: 'NetworkTool', href: '../basis/tools.html#class-NetworkTool', type: 'static' },
          { label: 'WindowsTool', href: '../basis/tools.html#class-WindowsTool', type: 'static' },
        ]
      },
      {
        label: 'Utils',
        children: [
          { label: 'EasyTask', href: '../basis/utils.html#class-EasyTask', type: 'class' },
          { label: 'ExternalProgram', href: '../basis/utils.html#class-ExternalProgram', type: 'class' },
          { label: 'HiResTimer', href: '../basis/utils.html#class-HiResTimer', type: 'class' },
          { label: 'NaturalSortComparer', href: '../basis/utils.html#class-NaturalSortComparer', type: 'class' },
          { label: 'NaturalSortExtensions', href: '../basis/utils.html#class-NaturalSortExtensions', type: 'static' },
          { label: 'CompressionUtility', href: '../basis/utils.html#class-CompressionUtility', type: 'static' },
        ]
      },
    ]
  },
  ui: {
    label: 'Going.UI',
    pkg: 'Going.UI',
    items: [
      { label: 'Overview', href: '../ui/index.html', type: 'page' },
      {
        label: 'Design',
        children: [
          { label: 'GoDesign', href: '../ui/design.html#class-GoDesign', type: 'class' },
          { label: 'GoWindow', href: '../ui/design.html#class-GoWindow', type: 'class' },
          { label: 'GoPage', href: '../ui/design.html#class-GoPage', type: 'class' },
          { label: 'GoTitleBar', href: '../ui/design.html#class-GoTitleBar', type: 'class' },
          { label: 'GoSideBar', href: '../ui/design.html#class-GoSideBar', type: 'class' },
          { label: 'GoFooter', href: '../ui/design.html#class-GoFooter', type: 'class' },
          { label: 'GoDropDownWindow', href: '../ui/design.html#class-GoDropDownWindow', type: 'class' },
        ]
      },
      {
        label: 'Controls',
        children: [
          { label: 'IGoControl', href: '../ui/controls.html#class-IGoControl', type: 'interface' },
          { label: 'GoControl', href: '../ui/controls.html#class-GoControl', type: 'class' },
          { label: 'GoButton', href: '../ui/controls.html#class-GoButton', type: 'class' },
          { label: 'GoLabel', href: '../ui/controls.html#class-GoLabel', type: 'class' },
          { label: 'GoCheckBox', href: '../ui/controls.html#class-GoCheckBox', type: 'class' },
          { label: 'GoRadioButton', href: '../ui/controls.html#class-GoRadioButton', type: 'class' },
          { label: 'GoSwitch', href: '../ui/controls.html#class-GoSwitch', type: 'class' },
          { label: 'GoToggleButton', href: '../ui/controls.html#class-GoToggleButton', type: 'class' },
          { label: 'GoInput', href: '../ui/controls.html#class-GoInput', type: 'class' },
          { label: 'GoNumberBox', href: '../ui/controls.html#class-GoNumberBox', type: 'class' },
          { label: 'GoSlider', href: '../ui/controls.html#class-GoSlider', type: 'class' },
          { label: 'GoRangeSlider', href: '../ui/controls.html#class-GoRangeSlider', type: 'class' },
          { label: 'GoKnob', href: '../ui/controls.html#class-GoKnob', type: 'class' },
          { label: 'GoOnOff', href: '../ui/controls.html#class-GoOnOff', type: 'class' },
          { label: 'GoLamp', href: '../ui/controls.html#class-GoLamp', type: 'class' },
          { label: 'GoLampButton', href: '../ui/controls.html#class-GoLampButton', type: 'class' },
          { label: 'GoIconButton', href: '../ui/controls.html#class-GoIconButton', type: 'class' },
          { label: 'GoStep', href: '../ui/controls.html#class-GoStep', type: 'class' },
          { label: 'GoValue', href: '../ui/controls.html#class-GoValue', type: 'class' },
          { label: 'GoListBox', href: '../ui/controls.html#class-GoListBox', type: 'class' },
          { label: 'GoDataGrid', href: '../ui/controls.html#class-GoDataGrid', type: 'class' },
          { label: 'GoTreeView', href: '../ui/controls.html#class-GoTreeView', type: 'class' },
          { label: 'GoCalendar', href: '../ui/controls.html#class-GoCalendar', type: 'class' },
          { label: 'GoColorSelector', href: '../ui/controls.html#class-GoColorSelector', type: 'class' },
          { label: 'GoButtons', href: '../ui/controls.html#class-GoButtons', type: 'class' },
          { label: 'GoProgress', href: '../ui/controls.html#class-GoProgress', type: 'class' },
          { label: 'GoNavigator', href: '../ui/controls.html#class-GoNavigator', type: 'class' },
          { label: 'GoLineGraph', href: '../ui/controls.html#class-GoLineGraph', type: 'class' },
          { label: 'GoBarGraph', href: '../ui/controls.html#class-GoBarGraph', type: 'class' },
          { label: 'GoCircleGraph', href: '../ui/controls.html#class-GoCircleGraph', type: 'class' },
          { label: 'GoGauge', href: '../ui/controls.html#class-GoGauge', type: 'class' },
          { label: 'GoPicture', href: '../ui/controls.html#class-GoPicture', type: 'class' },
          { label: 'GoAnimate', href: '../ui/controls.html#class-GoAnimate', type: 'class' },
        ]
      },
      {
        label: 'Containers',
        children: [
          { label: 'IGoContainer', href: '../ui/containers.html#class-IGoContainer', type: 'interface' },
          { label: 'GoContainer', href: '../ui/containers.html#class-GoContainer', type: 'class' },
          { label: 'GoPanel', href: '../ui/containers.html#class-GoPanel', type: 'class' },
          { label: 'GoBoxPanel', href: '../ui/containers.html#class-GoBoxPanel', type: 'class' },
          { label: 'GoGridLayoutPanel', href: '../ui/containers.html#class-GoGridLayoutPanel', type: 'class' },
          { label: 'GoGroupBox', href: '../ui/containers.html#class-GoGroupBox', type: 'class' },
          { label: 'GoPicturePanel', href: '../ui/containers.html#class-GoPicturePanel', type: 'class' },
          { label: 'GoScalePanel', href: '../ui/containers.html#class-GoScalePanel', type: 'class' },
          { label: 'GoScrollablePanel', href: '../ui/containers.html#class-GoScrollablePanel', type: 'class' },
          { label: 'GoSwitchPanel', href: '../ui/containers.html#class-GoSwitchPanel', type: 'class' },
          { label: 'GoTabControl', href: '../ui/containers.html#class-GoTabControl', type: 'class' },
          { label: 'GoTableLayoutPanel', href: '../ui/containers.html#class-GoTableLayoutPanel', type: 'class' },
        ]
      },
      {
        label: 'Dialogs',
        children: [
          { label: 'GoDialogs', href: '../ui/dialogs.html#class-GoDialogs', type: 'static' },
          { label: 'GoMessageBox', href: '../ui/dialogs.html#class-GoMessageBox', type: 'class' },
          { label: 'GoInputBox', href: '../ui/dialogs.html#class-GoInputBox', type: 'class' },
          { label: 'GoSelectorBox', href: '../ui/dialogs.html#class-GoSelectorBox', type: 'class' },
        ]
      },
      {
        label: 'Datas',
        children: [
          { label: 'GoListItem', href: '../ui/datas.html#class-GoListItem', type: 'class' },
          { label: 'GoPadding', href: '../ui/datas.html#class-GoPadding', type: 'class' },
          { label: 'GoButtonItem', href: '../ui/datas.html#class-GoButtonItem', type: 'class' },
          { label: 'GoMouseEventArgs', href: '../ui/datas.html#class-GoMouseEventArgs', type: 'class' },
          { label: 'GoMouseClickEventArgs', href: '../ui/datas.html#class-GoMouseClickEventArgs', type: 'class' },
          { label: 'GoCancelableEventArgs', href: '../ui/datas.html#class-GoCancelableEventArgs', type: 'class' },
          { label: 'GoDragEventArgs', href: '../ui/datas.html#class-GoDragEventArgs', type: 'class' },
        ]
      },
      {
        label: 'Enums',
        children: [
          { label: 'GoContentAlignment', href: '../ui/enums.html#enum-GoContentAlignment', type: 'enum' },
          { label: 'GoDirection', href: '../ui/enums.html#enum-GoDirection', type: 'enum' },
          { label: 'GoDockStyle', href: '../ui/enums.html#enum-GoDockStyle', type: 'enum' },
          { label: 'GoFontStyle', href: '../ui/enums.html#enum-GoFontStyle', type: 'enum' },
          { label: 'GoRoundType', href: '../ui/enums.html#enum-GoRoundType', type: 'enum' },
          { label: 'GoDialogResult', href: '../ui/enums.html#enum-GoDialogResult', type: 'enum' },
          { label: 'GoMouseButton', href: '../ui/enums.html#enum-GoMouseButton', type: 'enum' },
          { label: 'GoKeys', href: '../ui/enums.html#enum-GoKeys', type: 'enum' },
        ]
      },
      {
        label: 'Themes',
        children: [
          { label: 'GoTheme', href: '../ui/themes.html#class-GoTheme', type: 'class' },
        ]
      },
      {
        label: 'Collections',
        children: [
          { label: 'ObservableList&lt;T&gt;', href: '../ui/collections.html#class-ObservableList', type: 'class' },
        ]
      },
      {
        label: 'ImageCanvas',
        children: [
          { label: 'IcButton', href: '../ui/imagecanvas.html#class-IcButton', type: 'class' },
          { label: 'IcLabel', href: '../ui/imagecanvas.html#class-IcLabel', type: 'class' },
          { label: 'IcOnOff', href: '../ui/imagecanvas.html#class-IcOnOff', type: 'class' },
          { label: 'IcProgress', href: '../ui/imagecanvas.html#class-IcProgress', type: 'class' },
          { label: 'IcSlider', href: '../ui/imagecanvas.html#class-IcSlider', type: 'class' },
          { label: 'IcState', href: '../ui/imagecanvas.html#class-IcState', type: 'class' },
        ]
      },
    ]
  }
};

// ─── Build Sidebar HTML ─────────────────────────────────────────────
function buildSidebar(currentFile) {
  const basePath = getBasePath(currentFile);

  let html = `<a class="sidebar-logo" href="${basePath}index.html" style="display:flex;align-items:center;gap:8px;padding:14px;text-decoration:none;color:#d4d4d4;font-size:13px;font-weight:600;border-bottom:1px solid var(--border);">
    <span style="background:var(--accent);color:#fff;font-size:10px;font-weight:700;padding:2px 6px;border-radius:3px;">API</span>
    Going Library
  </a>`;

  for (const [key, proj] of Object.entries(NAV)) {
    html += `<div class="sidebar-section">
      <div class="sidebar-group-header" onclick="toggleGroup(this)">
        <span style="color:var(--keyword)">${proj.label}</span>
        <span class="toggle-icon">▼</span>
      </div>
      <div class="sidebar-group-body">`;

    for (const item of proj.items) {
      if (!item.children) {
        const href = resolveHref(item.href, basePath);
        html += `<a class="sidebar-item type-page" href="${href}" style="padding-left:16px">${item.label}</a>`;
      } else {
        html += `<div class="sidebar-ns">
          <div class="sidebar-ns-header" onclick="toggleNs(this)">
            <span class="ns-icon">▼</span>
            ${item.label}
          </div>
          <div class="sidebar-ns-items">`;
        for (const child of item.children) {
          const href = resolveHref(child.href, basePath);
          const active = isActive(child.href, currentFile) ? ' active' : '';
          html += `<a class="sidebar-item type-${child.type}${active}" href="${href}">${child.label}</a>`;
        }
        html += `</div></div>`;
      }
    }

    html += `</div></div>`;
  }

  return html;
}

function getBasePath(currentFile) {
  if (!currentFile) return './';
  const depth = (currentFile.match(/\//g) || []).length;
  return depth > 0 ? '../'.repeat(depth) : './';
}

function resolveHref(href, basePath) {
  if (href.startsWith('../')) return basePath + href.slice(3);
  return basePath + href;
}

function isActive(href, currentFile) {
  if (!currentFile) return false;
  return href.includes(currentFile.split('/').pop().split('#')[0]);
}

// ─── Toggle Helpers ─────────────────────────────────────────────────
function toggleGroup(header) {
  header.classList.toggle('collapsed');
  const body = header.nextElementSibling;
  body.style.display = body.style.display === 'none' ? '' : 'none';
}

function toggleNs(header) {
  header.classList.toggle('collapsed');
  const items = header.nextElementSibling;
  items.style.display = items.style.display === 'none' ? '' : 'none';
}

// ─── Search / Filter ────────────────────────────────────────────────
function initSearch() {
  const input = document.getElementById('search-input');
  if (!input) return;

  input.addEventListener('input', () => {
    const q = input.value.trim().toLowerCase();
    const cards = document.querySelectorAll('.class-card');
    let anyVisible = false;

    cards.forEach(card => {
      if (!q) {
        card.classList.remove('search-hidden');
        anyVisible = true;
        // Show all rows
        card.querySelectorAll('tr[data-search]').forEach(tr => tr.classList.remove('search-hidden'));
        return;
      }

      const cardText = card.getAttribute('data-search') || '';
      const cardMatch = cardText.includes(q);

      // Check row-level matches too
      const rows = card.querySelectorAll('tr[data-search]');
      let rowMatch = false;
      rows.forEach(tr => {
        const t = (tr.getAttribute('data-search') || '').toLowerCase();
        if (t.includes(q)) {
          tr.classList.remove('search-hidden');
          rowMatch = true;
        } else {
          tr.classList.add('search-hidden');
        }
      });

      if (cardMatch || rowMatch) {
        card.classList.remove('search-hidden');
        anyVisible = true;
      } else {
        card.classList.add('search-hidden');
      }
    });

    const noResults = document.getElementById('no-results');
    if (noResults) noResults.style.display = anyVisible ? 'none' : 'block';
  });
}

// ─── Scroll Spy ─────────────────────────────────────────────────────
function initScrollSpy() {
  const cards = document.querySelectorAll('.class-card[id]');
  if (!cards.length) return;

  const observer = new IntersectionObserver((entries) => {
    entries.forEach(entry => {
      if (entry.isIntersecting) {
        const id = entry.target.id;
        document.querySelectorAll('.sidebar-item').forEach(a => {
          a.classList.toggle('active', a.getAttribute('href')?.includes('#' + id));
        });
      }
    });
  }, { rootMargin: '-20% 0px -70% 0px' });

  cards.forEach(c => observer.observe(c));
}

// ─── Back to Top ────────────────────────────────────────────────────
function initBackToTop() {
  const btn = document.getElementById('back-to-top');
  if (!btn) return;
  window.addEventListener('scroll', () => {
    btn.classList.toggle('visible', window.scrollY > 300);
  });
  btn.addEventListener('click', () => window.scrollTo({ top: 0, behavior: 'smooth' }));
}

// ─── Toggle class card body ─────────────────────────────────────────
function toggleCard(header) {
  const body = header.nextElementSibling;
  if (!body) return;
  const isOpen = body.style.display !== 'none';
  body.style.display = isOpen ? 'none' : '';
  header.querySelector('.card-toggle')?.classList.toggle('collapsed', isOpen);
}

// ─── Init on DOMContentLoaded ────────────────────────────────────────
document.addEventListener('DOMContentLoaded', () => {
  // Inject sidebar
  const sidebarEl = document.getElementById('sidebar');
  if (sidebarEl) {
    const currentFile = sidebarEl.getAttribute('data-current') || '';
    sidebarEl.innerHTML = buildSidebar(currentFile);
  }

  initSearch();
  initScrollSpy();
  initBackToTop();
});
