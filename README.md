# BANDWAY

## Implémentation de l'algorithme de Dijkstra

Cette application a été développée dans le cadre d’un séminaire de développement logiciel avancé.

Le choix de la technologie s’est porté sur Unity, afin de faciliter la mise en place de l’interface utilisateur et les interactions avec celle-ci. L’utilisation de la 3D a également motivé ce choix.

## Pour modifier le fichier JSON :

Modifiez le fichier **GeoJson.json** situé dans le dossier `Build\BandWay_Data\StreamingAssets`.  
Ce fichier doit être au format **GeoJSON** et contenir des points sous la forme suivante :

```json
{
  "type": "Feature",
  "properties": {
    "name": "Ajaccio"
  },
  "geometry": {
    "type": "Point",
    "coordinates": [1431.8, -1325.5]
  }
}
```