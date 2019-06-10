{
	"Analisis": {
		"nombre_Proyecto": "Proyecto de prueba",
		"fecha_Procesamiento": "Wed Jan 01 2014 13:28:56",
		"ruta_Proyecto": "C:\\Users\\Steven\\Documents\\GitHub\\avibanalysis\\Code-Cloner Java\\Files\\Output",
		"cantidad_Clones_Tipo_1": 2,
		"cantidad_Clones_Tipo_2": 1,
		"cantidad_Clones_Tipo_3": 2,
		"Tamano_Minimo_Clones": 3, 
		"Granularidad":"Sentencias",
		"Clones" : [
			{
				"Tipo": "Clon Tipo 1",
				"CantidadCopias": 3,
				"Copias":[
					{
						"ID":"Clase+Metodo+numeroConsecutivo",
						"Package":"paquete",
						"Metodo" : "main",
						"Codigo" : "System.out.printLn(Hola Mundo)",
						"Clase" : "Inicio",
						"Numero_linea" : 2
					},
					{
						"ID":"Clase+Metodo+numeroConsecutivo",
						"Package":"paquete",
						"Metodo" : "prueba",
						"Codigo" : "System.out.printLn(Hola Mundo)",
						"Clase" : "Inicio",
						"Numero_linea" : 9
					},
					{
						"ID":"Clase+Metodo+numeroConsecutivo",
						"Package":"paquete",
						"Metodo" : "main",
						"Codigo" : "System.out.printLn(Hola Mundo)",
						"Clase" : "Perro",
						"Numero_linea" : 5
					}
				]
					
				},
				{
						"Tipo": "Clon Tipo 1",
						"CantidadCopias": 3,
						"Copias":[
							{
								"ID":"Package+Clase+Metodo+numeroConsecutivo",
								"Package":"paquete",
								"Metodo" : "main",
								"Codigo" : "for(int i = 2; i <= 100; i++){resultado *= i;}",
								"Clase" : "Inicio",
								"Numero_linea" : 15
							},
							{
								"ID":"Package+Clase+Metodo+numeroConsecutivo",
								"Package":"paquete",
								"Metodo" : "prueba",
								"Codigo" : "for(int i = 2; i <= 100; i++){resultado *= i;}",
								"Clase" : "Inicio",
								"Numero_linea" : 19
							},
							{
								"ID":"Package+Clase+Metodo+numeroConsecutivo",
								"Package":"paquete",
								"Metodo" : "main",
								"Codigo" : "for(int i = 2; i <= 100; i++){resultado *= i;}",
								"Clase" : "Perro",
								"Numero_linea" : 35
							}
						]
					
				},
				{
					"Tipo": "Clon Tipo 2",
					"CantidadCopias": 3,
					"Copias":[
						{
							"ID":"Package+Clase+Metodo+numeroConsecutivo",
							"Package":"paquete",
							"Metodo" : "main",
							"Codigo" : "Scanner s = new Scanner( new File(scores.dat) );",
							"Clase" : "Inicio",
							"Numero_linea" : 2
						},
						{
							"ID":"Package+Clase+Metodo+numeroConsecutivo",
							"Package":"paquete",
							"Metodo" : "prueba",
							"Codigo" : "Scanner j = new Scanner( new File(scores.dat) );",
							"Clase" : "Inicio",
							"Numero_linea" : 9
						},
						{
							"ID":"Package+Clase+Metodo+numeroConsecutivo",
							"Package":"paquete",
							"Metodo" : "main",
							"Codigo" : "Scanner l = new Scanner( new File(scores.dat) );",
							"Clase" : "Perro",
							"Numero_linea" : 5
						}
					]
						
				}
		]
	}
}
			