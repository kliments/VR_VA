# VR_VA
VR visual analytics tool for collaborative environment. When loaded, multi-dimensional data is reduced to 3 dimensions using PCA. Data points can be visualized in 4 different types: 
  * cubes
  * pie-charts (each part of the pie references the value of each dimension)
  * triangles (the distance from the center to each edge references the value of each dimension)
  * tetrahedrons (same as the triangles, with 4th dimension with default size)
  
Clustering algorithms included:
  * K-means
  * DBSCAN
  * Denclue

Multi VR environment where the the server is "Professor" and clients are "Students". Permissions for loading,visualizing and running clustering algorithms can be given from the server to each of the clients.
