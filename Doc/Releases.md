# Releases of Arc4u packages.



## 6.0

### 6.0.9.1
=> packages updated to the version 6.0.9 of dotNet framework.

### 6.0.8.2
=> Fix a severe security issue regarding the ServiceAspect used to control the right of a user. See comment on 5.0.17.3

## 5.0

### 5.0.17.3
=> Fix a severe security issue regarding the ServiceAspect used to control the right of a user.
=> From dotNet 5.0 the Attributes are not loaded contextually for each call but loaded once. To have the context of the user the injection of the IApplicationContext will not work. The applicationContext will be the context of the first user calling an api!
