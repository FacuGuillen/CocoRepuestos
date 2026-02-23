let ventasGlobales = []; // Esta es la variable que usa el Modal

const API_URL = 'https://localhost:7095/api';

async function obtenerReporte() {
    try {
        const response = await fetch(`${API_URL}/RepuestosVentas`);
        if (!response.ok) throw new Error("No se pudo obtener el historial");

        // 1. Obtenemos los datos crudos de la API
        const datosApi = await response.json();
        
        // ¡¡AQUI ESTABA EL ERROR!! 
        // Guardamos los datos en la variable GLOBAL para que el modal los pueda usar
        ventasGlobales = datosApi; 

        // 2. Ahora preparamos los datos para la TABLA (Filtrado visual)
        let ventasParaTabla = [...ventasGlobales]; // Hacemos una copia para no romper la global

        const mesFiltro = document.getElementById('filtro-mes').value; // Formato 2026-02
        const clienteFiltro = document.getElementById('filtro-cliente').value.trim().toLowerCase();

        // Filtro por MES en la tabla
        if (mesFiltro) {
            ventasParaTabla = ventasParaTabla.filter(v => {
                const f = v.fechaVenta || v.fecha || "";
                return f.startsWith(mesFiltro);
            });
        }

        // Filtro por CLIENTE en la tabla
        if (clienteFiltro) {
            ventasParaTabla = ventasParaTabla.filter(v => {
                const nombreEnDB = v.cliente?.nombre || ""; 
                return nombreEnDB.toLowerCase().includes(clienteFiltro);
            });
        }

        renderizarTabla(ventasParaTabla);

    } catch (error) {
        console.error("Error:", error);
        alert("Error al cargar el reporte. Verifica que la API esté corriendo.");
    }
}

function renderizarTabla(ventas) {
    const tabla = document.getElementById('tabla-ventas');
    const totalDisplay = document.getElementById('monto-total-ganancia');
    let html = '';
    let gananciaTotalAcumulada = 0;

    if (ventas.length === 0) {
        tabla.innerHTML = '<tr><td colspan="6" class="text-center text-muted">No se encontraron ventas con estos filtros</td></tr>';
        totalDisplay.innerText = "$ 0,00";
        return;
    }

    ventas.forEach(v => {
        const costoUnitarioUSD = v.repuestoStock?.costoUSD || 0;
        const tasa = v.tasaCambio || 1;
        const cantidad = v.cantidadVendida || 1;
        
        // Precio cobrado final
        const precioUnitarioCobrado = v.precioVentaUnitarioEnARS || 0;
        
        // Cálculos
        const ingresoTotalARS = precioUnitarioCobrado * cantidad;
        const costoTotalARS = (costoUnitarioUSD * tasa) * cantidad;
        const gananciaIndividual = ingresoTotalARS - costoTotalARS;
        
        gananciaTotalAcumulada += gananciaIndividual;

        const fechaRaw = v.fechaVenta || v.fecha;
        const colorGanancia = gananciaIndividual >= 0 ? 'text-success' : 'text-danger';

        // Fix visual para fecha
        let fechaVisual = 'S/F';
        if(fechaRaw) {
            const dateObj = new Date(fechaRaw);
            fechaVisual = dateObj.toLocaleDateString('es-AR');
        }

        html += `
            <tr>
                <td>${fechaVisual}</td>
                <td>${v.repuestoStock?.descripcion || 'Repuesto desconocido'}</td>
                <td class="fw-bold">$ ${ingresoTotalARS.toLocaleString('es-AR')}</td>
                <td class="text-center">${cantidad}</td>
                <td class="${colorGanancia} fw-bold">
                    ${gananciaIndividual >= 0 ? '+' : ''}$ ${gananciaIndividual.toLocaleString('es-AR', {minimumFractionDigits: 2})}
                </td>
                <td><span class="badge bg-info text-dark">${v.provincia || 'Sin Prov.'}</span></td>
            </tr>
        `;
    });

    tabla.innerHTML = html;
    totalDisplay.innerText = `$ ${gananciaTotalAcumulada.toLocaleString('es-AR', {minimumFractionDigits: 2})}`;
}

function limpiarFiltro() {
    document.getElementById('filtro-mes').value = '';
    document.getElementById('filtro-cliente').value = '';
    obtenerReporte(); // Recarga todo
}

// --- LOGICA DEL MODAL SIFERE ---

function abrirModalConvenio() {
    const modal = new bootstrap.Modal(document.getElementById('modalConvenio'));
    modal.show();
}

function calcularSifereEnModal() {
    const mesSeleccionado = document.getElementById('mes-sifere').value; 
    const provinciaSeleccionada = document.getElementById('filtro-provincia-modal').value; 

    if (!mesSeleccionado) return alert("Selecciona un mes");

    // DEBUG: Ahora esto debería mostrar un número mayor a 0 en la consola (F12)
    console.log("Calculando para:", mesSeleccionado, provinciaSeleccionada);
    console.log("Total ventas en memoria:", ventasGlobales.length);

    // Elementos del DOM
    const contenedorResultado = document.getElementById('resultado-sifere-body');
    const contenedorUnico = document.getElementById('resultado-unico');
    const contenedorTabla = document.getElementById('tabla-resultado-container');
    const tbody = document.getElementById('lista-provincias-modal');

    // Limpiamos
    tbody.innerHTML = '';
    let totalGeneral = 0;
    const agrupado = {};

    // Normalizamos la selección del usuario
    const filtroNormalizado = provinciaSeleccionada.trim().toLowerCase();

    ventasGlobales.forEach(v => {
        // Extraer fecha YYYY-MM
        const fechaVenta = v.fechaVenta.split('T')[0].substring(0, 7);
        
        // Normalizamos la provincia que viene de la Base de Datos
        let provData = (v.provincia || 'Sin Provincia Asignada').toString();
        const provDataNormalizada = provData.trim().toLowerCase();

        // LÓGICA DE FILTRO
        if (fechaVenta === mesSeleccionado) {
            
            // Si NO eligió "todas", comparamos los textos normalizados
            if (filtroNormalizado !== 'todas' && provDataNormalizada !== filtroNormalizado) {
                return; // Saltamos si no coinciden
            }

            // Calculamos monto (Precio * Cantidad)
            const monto = (v.precioVentaUnitarioEnARS || 0) * (v.cantidadVendida || 1);

            // Guardamos
            if (!agrupado[provData]) {
                agrupado[provData] = 0;
            }
            agrupado[provData] += monto;
            totalGeneral += monto;
        }
    });

    // MOSTRAR RESULTADOS
    contenedorResultado.style.display = 'block';

    // CASO A: Eligió una provincia específica
    if (provinciaSeleccionada !== 'TODAS') {
        contenedorTabla.style.display = 'none';
        contenedorUnico.style.display = 'block';

        document.getElementById('titulo-resultado-unico').innerText = `VENTAS A ${provinciaSeleccionada.toUpperCase()}`;
        document.getElementById('monto-resultado-unico').innerText = `$${totalGeneral.toLocaleString('es-AR', {minimumFractionDigits: 2})}`;
        
        const elementoMonto = document.getElementById('monto-resultado-unico');
        if (totalGeneral > 0) {
             elementoMonto.className = "fw-bold text-success display-6";
        } else {
             elementoMonto.className = "fw-bold text-muted display-6";
             elementoMonto.innerText = "Sin ventas ($0)";
        }
    } 
    // CASO B: Eligió "TODAS"
    else {
        contenedorUnico.style.display = 'none';
        contenedorTabla.style.display = 'block';

        if (totalGeneral === 0) {
            tbody.innerHTML = '<tr><td colspan="2" class="text-center text-danger">No hay ventas registradas en este mes.</td></tr>';
        } else {
            const listaOrdenada = Object.entries(agrupado).sort((a, b) => b[1] - a[1]);
            let htmlFilas = '';
            listaOrdenada.forEach(([provincia, monto]) => {
                htmlFilas += `
                    <tr>
                        <td>${provincia}</td>
                        <td class="text-end fw-bold">$${monto.toLocaleString('es-AR', {minimumFractionDigits: 2})}</td>
                    </tr>
                `;
            });
            tbody.innerHTML = htmlFilas;
        }
        document.getElementById('total-sifere-modal').innerText = `$${totalGeneral.toLocaleString('es-AR', {minimumFractionDigits: 2})}`;
    }
}

// Cargar al iniciar
window.onload = obtenerReporte;