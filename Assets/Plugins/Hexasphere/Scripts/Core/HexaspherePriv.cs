/// <summary>
/// Hexasphere Grid System
/// Created by Ramiro Oliva (Kronnect)
/// </summary>


//#define VR_GOOGLE				  	 							    // Uncomment this line to support Google VR SDK (pointer and controller touch)
//#define VR_SAMSUNG_GEAR_CONTROLLER  // Uncomment this line to support Samsung Gear VR SDK (laser pointer)

//#define TRACE_PERFORMANCE    	      // Used to track performance metrics. Internal use.
//#define RAYCAST3D_DEBUG             // Used to debug raycasting in extrusion mode. Internal use.

using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif
#if VR_GOOGLE
using GVR;
#endif
using System;
using System.Collections;
using System.Collections.Generic;


namespace HexasphereGrid {
    public delegate Point GetCachedPointDelegate(Point point);

    [ExecuteInEditMode]
    public partial class Hexasphere : MonoBehaviour {

        const float MIN_FIELD_OF_VIEW = 10.0f;
        const float MAX_FIELD_OF_VIEW = 85.0f;
        const int MAX_TEXTURES = 255;

        Material _tileShadedFrameMatBevel, _tileShadedFrameMatExtrusion, _tileShadedFrameMatNoExtrusion;
        Material _gridMatExtrusion, _gridMatNoExtrusion;
        Material _tileColoredMat, _tileTexturedMat;
        Material highlightMaterial;
        int currentDivisions, currentTextureSize;
        bool currentExtruded, currentInvertedMode, currentWireframeColorFromTile, currentSmartEdges, currentBevel;
        Color currentDefaultShadedColor;
        bool pendingUVUpdateFast, pendingTextureArrayUpdate, pendingColorsUpdate;
        STYLE currentStyle;
        float currentTransparencyTiles;
        bool mouseIsOver, mouseStartedDragging, hasDragged;
        float clickStart;
#if VR_GOOGLE
        Vector3 mouseDragStartScreenPos;
#endif
        Vector3 mouseDragStartLocalPosition;
        float wheelAccel;
        Quaternion flyingStartRotation, flyingEndRotation;
        bool flying;
        float flyingStartTime, flyingDuration;
        Texture2D defaultRampTexture;
        SphereCollider sphereCollider;
        int lastHitTileIndex;
        Texture2D whiteTex;
        int uvChunkCount, wireChunkCount;
        Vector3 currentRotationShift;
        bool leftMouseButtonClick, leftMouseButtonPressed, leftMouseButtonRelease;
        bool rightMouseButtonPressed;
        bool allowedTextureArray;
        bool useEditorRay;
        Ray editorRay;
        bool shouldUpdateMaterialProperties;
        bool needRegenerate, needRegenerateWireframe;
        Texture2D bevelNormals;
        Color[] bevelNormalsColors;
        List<int> tmpList;
        Dictionary<int, bool> tmpDict;
        List<int> tmpCandidates;
        int lastHoverTileIndex;

        #region Gameloop events

        void OnEnable() {
            if (!Application.isPlaying) {
                CheckCamera();
            }
            Init();
        }

        void Start() {
            RegisterVRPointers();
            CheckCamera();
        }

        void OnDestroy() {
            if (_gridMatExtrusion != null)
                DestroyImmediate(_gridMatExtrusion);
            if (_gridMatNoExtrusion != null)
                DestroyImmediate(_gridMatNoExtrusion);
            if (_tileShadedFrameMatBevel != null)
                DestroyImmediate(_tileShadedFrameMatBevel);
            if (_tileShadedFrameMatExtrusion != null)
                DestroyImmediate(_tileShadedFrameMatExtrusion);
            if (_tileShadedFrameMatNoExtrusion != null)
                DestroyImmediate(_tileShadedFrameMatNoExtrusion);
            if (_tileColoredMat != null)
                DestroyImmediate(_tileColoredMat);
            if (_tileColoredMat != null)
                DestroyImmediate(_tileColoredMat);
            if (_tileTexturedMat != null)
                DestroyImmediate(_tileTexturedMat);
        }

        void LateUpdate() {

#if RAYCAST3D_DEBUG
												if (Input.GetKeyDown (KeyCode.D))
																rayDebug = true;
#endif
            if (shouldUpdateMaterialProperties) {
                UpdateMaterialProperties();
            }
            if (pendingTextureArrayUpdate) {
                UpdateShadedMaterials();
                pendingUVUpdateFast = false;
            } else if (pendingUVUpdateFast || pendingColorsUpdate) {
                UpdateShadedMaterialsFast();
                UpdateWireMaterialsFast();
                pendingUVUpdateFast = false;
            }
            if (pendingUVUpdateFast) {
                UpdateWireMaterialsFast();
                pendingUVUpdateFast = false;
            }

            if (highlightMaterial != null && lastHighlightedTileIndex >= 0) {
                highlightMaterial.SetFloat("_ColorShift", Mathf.PingPong(Time.time * _highlightSpeed, 1f));
            }

            // Check mouse buttons state
            if (_rightButtonDrag) {
                leftMouseButtonClick = Input.GetMouseButtonDown(1);
            } else {
                leftMouseButtonClick = Input.GetMouseButtonDown(0) || Input.GetButtonDown("Fire1");
            }
#if VR_GOOGLE
												if (GvrController.TouchDown) {
												GVR_TouchStarted = true;
												leftMouseButtonClick = true;
												}
#endif

            if (_rightButtonDrag) {
                leftMouseButtonPressed = leftMouseButtonClick || Input.GetMouseButton(1);
            } else {
                leftMouseButtonPressed = leftMouseButtonClick || Input.GetMouseButton(0);
            }
#if VR_GOOGLE
												if (GVR_TouchStarted)
												leftMouseButtonPressed = true;
#endif

            if (_rightButtonDrag) {
                leftMouseButtonRelease = Input.GetMouseButtonUp(1);
            } else {
                leftMouseButtonRelease = Input.GetMouseButtonUp(0) || Input.GetButtonUp("Fire1");
            }
#if VR_GOOGLE
												if (GvrController.TouchUp) {
												GVR_TouchStarted = false;
												leftMouseButtonRelease = true;
												}
#endif

            rightMouseButtonPressed = Input.GetMouseButton(1) && !_rightButtonDrag;

            // Check whether the points is on an UI element, then avoid user interaction
            bool canInteract = true;
            if (respectOtherUI) {
                if (UnityEngine.EventSystems.EventSystem.current != null) {
                    if (Application.isMobilePlatform && Input.touchCount > 0 && UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId)) {
                        canInteract = false;
                    } else if (UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject(-1))
                        canInteract = false;
                }
                if (!canInteract) {
                    HideHighlightedTile();
                }
            }

            if (canInteract) {
                if (_invertedMode) {
                    CheckUserInteractionInvertedMode();
                } else if (mouseIsOver || _VREnabled) {
                    CheckUserInteractionNormalMode();
                }
            }

            if (flying) {
                float t = (Time.time - flyingStartTime) / flyingDuration;
                t = Mathf.Clamp01(t);
                transform.rotation = Quaternion.Slerp(flyingStartRotation, flyingEndRotation, t);
                if (t >= 1) {
                    flying = false;
                }
                if (_rotationAxisAllowed == ROTATION_AXIS_ALLOWED.STRAIGHT) {
                    KeepStraight();
                }
            }
        }

        void OnMouseEnter() {
            mouseIsOver = true;
        }

        void OnMouseExit() {

            if (_VREnabled)
                return;

            // Check if it's really outside of hexasphere
            Vector3 dummy;
            Ray dummyRay;
            if (!GetHitPoint(out dummy, out dummyRay)) {
                mouseIsOver = false;
            }
            if (!mouseIsOver) {
                HideHighlightedTile();
            }
        }

        void FixedUpdate() {
            if (_style != STYLE.Shaded) {
                if (_gridMatExtrusion != null) {
                    _gridMatExtrusion.SetVector("_Center", transform.position);
                }
                if (_gridMatNoExtrusion != null) {
                    _gridMatNoExtrusion.SetVector("_Center", transform.position);
                }
            }
            if (_extruded && _style != STYLE.Wireframe) {
                if (_tileShadedFrameMatExtrusion != null) {
                    _tileShadedFrameMatExtrusion.SetVector("_Center", transform.position);
                }
                if (_tileShadedFrameMatBevel != null) {
                    _tileShadedFrameMatBevel.SetVector("_Center", transform.position);
                }
            }
        }


        #endregion

        #region Initialization

        public void Init() {
            sphereCollider = GetComponent<SphereCollider>();
            if (sphereCollider == null) {
                sphereCollider = gameObject.AddComponent<SphereCollider>();
            }
            if (highlightMaterial == null) {
                highlightMaterial = Resources.Load<Material>("Materials/HexaTileHighlightMat");
            }

            allowedTextureArray = SystemInfo.supports2DArrayTextures;
            if (!allowedTextureArray) {
                Debug.LogWarning("Current platform does not support array textures. Hexasphere shading won't work.");
            }

            if (!_invertedMode && _cameraMain != null)
                oldCameraPosition = _cameraMain.transform.position;

            if (textures == null || textures.Length < MAX_TEXTURES)
                textures = new Texture2D[MAX_TEXTURES];

            UpdateMaterialProperties();
        }

        void CheckCamera() {
            if (_cameraMain == null) {
                _cameraMain = Camera.main;
                if (_cameraMain == null) {
                    _cameraMain = FindObjectOfType<Camera>();
                    if (_cameraMain == null) {
                        Debug.LogWarning("No camera found!");
                    }
                }
            }
        }

        #endregion



        #region Interaction

        /// <summary>
        /// Issues a selection check based on a given ray. Used by editor to manipulate tiles from Scene window.
        /// Returns true if ray hits the grid.
        /// </summary>
        public bool CheckRay(Ray ray) {
            useEditorRay = true;
            editorRay = ray;
            Vector3 dummyPos;
            Ray dummyRay;
            if (_invertedMode) {
                CheckMousePosInvertedMode(out dummyPos, out dummyRay);
            } else {
                CheckMousePosNormalMode(out dummyPos, out dummyRay);
            }
            if (!mouseIsOver) {
                HideHighlightedTile();
            }
            return mouseIsOver;
        }

        void CheckUserInteractionNormalMode() {
            Vector3 position;
            Ray ray;

            CheckMousePosNormalMode(out position, out ray);

            if (_rotationEnabled) {
                if (leftMouseButtonClick) {
#if VR_GOOGLE
																				mouseDragStartScreenPos = GvrController.TouchPos;
#endif
                    mouseDragStartLocalPosition = transform.InverseTransformPoint(position);
                    mouseStartedDragging = true;
                    hasDragged = false;
                    clickStart = Time.time;
                } else if (mouseStartedDragging && (leftMouseButtonPressed || (Input.touchSupported && Input.touchCount == 1))) {
#if VR_GOOGLE
																				float distFactor = Mathf.Min (Vector3.Distance (_cameraMain.transform.position, transform.position) / transform.localScale.y, 1f);
																				Vector3 dragDirection = (mouseDragStartScreenPos - (Vector3)GvrController.TouchPos) * distFactor * _mouseDragSensitivity;
																					dragDirection.y *= -1.0f;
																					if (dragDirection.x != 0 || dragDirection.y != 0) {
																					hasDragged = true;
																				gameObject.transform.Rotate (Vector3.up, dragDirection.x, Space.World);
																				Vector3 axisY = Vector3.Cross (transform.position - _cameraMain.transform.position, Vector3.up);
																				transform.Rotate (axisY, dragDirection.y, Space.World);																				}
#else
                    Vector3 localPos = transform.InverseTransformPoint(position);
                    if (localPos != mouseDragStartLocalPosition) {
                        if (_rotationAxisAllowed == ROTATION_AXIS_ALLOWED.X_AXIS_ONLY) {
                            mouseDragStartLocalPosition.x = 0;
                            localPos.x = 0;
                        } else if (_rotationAxisAllowed == ROTATION_AXIS_ALLOWED.Y_AXIS_ONLY) {
                            mouseDragStartLocalPosition.y = 0;
                            localPos.y = 0;
                        }
                        float angle = Vector3.Angle(mouseDragStartLocalPosition, localPos);
                        Quaternion rot = Quaternion.AngleAxis(angle, Vector3.Cross(mouseDragStartLocalPosition, localPos));

                        if (_rotationSpeed < 1f) {
                            Quaternion newRot = transform.rotation * rot;
                            transform.rotation = Quaternion.Slerp(transform.rotation, newRot, _rotationSpeed);
                        } else {
                            transform.rotation *= rot;
                        }
                        // Keep straight
                        if (_rotationAxisAllowed == ROTATION_AXIS_ALLOWED.STRAIGHT) {
                            // Avoid rotation across poles
                            KeepStraight();
                            CheckMousePosNormalMode(out position, out ray);
                            mouseDragStartLocalPosition = transform.InverseTransformPoint(position);
                        }

                        if (angle > _dragThreshold) {
                            hasDragged = true;
                        }
                    }
#endif
                } else {
                    mouseStartedDragging = false;
                }
            }

            if (_rightClickRotates && rightMouseButtonPressed) {
                Vector3 axis = (transform.position - _cameraMain.transform.position).normalized;
                float rotAngle = _rightClickRotatingClockwise ? -2f : 2f;
                if (Input.GetKey(KeyCode.LeftAlt)) {
                    rotAngle *= -1;
                }
                transform.Rotate(axis, rotAngle, Space.World);
            }

            if (leftMouseButtonRelease && !hasDragged && (Time.time-clickStart <= _clickDuration) && OnTileClick != null) {
                OnTileClick(lastHoverTileIndex);
                lastClickedTile = lastHoverTileIndex;
            }

            if (_zoomEnabled) {
                // Use mouse wheel to zoom in and out
                float wheel = Input.GetAxis("Mouse ScrollWheel");
                wheelAccel += wheel;

                // Support for pinch on mobile
                if (Input.touchSupported && Input.touchCount == 2) {
                    // Store both touches.
                    Touch touchZero = Input.GetTouch(0);
                    Touch touchOne = Input.GetTouch(1);

                    // Find the position in the previous frame of each touch.
                    Vector2 touchZeroPrevPos = touchZero.position - touchZero.deltaPosition;
                    Vector2 touchOnePrevPos = touchOne.position - touchOne.deltaPosition;

                    // Find the magnitude of the vector (the distance) between the touches in each frame.
                    float prevTouchDeltaMag = (touchZeroPrevPos - touchOnePrevPos).magnitude;
                    float touchDeltaMag = (touchZero.position - touchOne.position).magnitude;

                    // Find the difference in the distances between each frame.
                    float deltaMagnitudeDiff = prevTouchDeltaMag - touchDeltaMag;

                    // Pass the delta to the wheel accel
                    wheelAccel += deltaMagnitudeDiff;
                }

                if (wheelAccel != 0) {
                    wheelAccel = Mathf.Clamp(wheelAccel, -0.1f, 0.1f);
                    if (wheelAccel >= 0.01f || wheelAccel <= -0.01f) {
                        Vector3 camPos = _cameraMain.transform.position - (transform.position - _cameraMain.transform.position) * wheelAccel * _zoomSpeed;
                        _cameraMain.transform.position = camPos;
                        float radiusSqr = (1.0f + _zoomMinDistance) * transform.localScale.z * 0.5f + (_cameraMain.nearClipPlane + 0.01f);
                        radiusSqr *= radiusSqr;
                        float camDistSqr = (_cameraMain.transform.position - transform.position).sqrMagnitude;
                        if (camDistSqr < radiusSqr) {
                            _cameraMain.transform.position = transform.position + (_cameraMain.transform.position - transform.position).normalized * Mathf.Sqrt(radiusSqr); // + 0.01f);
                            wheelAccel = 0;
                        } else {
                            radiusSqr = _zoomMaxDistance + transform.localScale.z * 0.5f + _cameraMain.nearClipPlane;
                            radiusSqr *= radiusSqr;
                            if (camDistSqr > radiusSqr) {
                                _cameraMain.transform.position = transform.position + (_cameraMain.transform.position - transform.position).normalized * Mathf.Sqrt(radiusSqr - 0.01f);
                                wheelAccel = 0;
                            }
                        }
                        wheelAccel *= _zoomDamping; // smooth dampening
                    }
                } else {
                    wheelAccel = 0;
                }
            }
        }

        void KeepStraight() {
            Vector3 v2 = -_cameraMain.transform.forward;
            Vector3 v3 = Vector3.ProjectOnPlane(transform.up, v2);
            float angle2 = SignedAngleBetween(_cameraMain.transform.up, v3, v2);
            transform.Rotate(v2, -angle2, Space.World);

            angle2 = SignedAngleBetween(transform.up, v2, transform.right);
            if (angle2 > 0 && angle2 < _rotationAxisVerticalThreshold) {
                transform.Rotate(_cameraMain.transform.right, _rotationAxisVerticalThreshold - angle2, Space.World);
            } else if (angle2 > -_rotationAxisVerticalThreshold && angle2 < 0) {
                transform.Rotate(_cameraMain.transform.right, _rotationAxisVerticalThreshold + angle2, Space.World);
            }
            if (angle2 > (180 - _rotationAxisVerticalThreshold) && angle2 < 180) {
                transform.Rotate(_cameraMain.transform.right, (180 - _rotationAxisVerticalThreshold) - angle2, Space.World);
            } else if (angle2 > -180 && angle2 < (-180 + _rotationAxisVerticalThreshold)) {
                transform.Rotate(_cameraMain.transform.right, (180 - _rotationAxisVerticalThreshold) + angle2, Space.World);
            }
        }

        float SignedAngleBetween(Vector3 a, Vector3 b, Vector3 n) {
            // angle in [0,180]
            float angle = Vector3.Angle(a, b);
            float sign = Mathf.Sign(Vector3.Dot(n, Vector3.Cross(a, b)));

            // angle in [-179,180]
            float signed_angle = angle * sign;

            return signed_angle;
        }



        void CheckUserInteractionInvertedMode() {
            Vector3 position;
            Ray ray;

            CheckMousePosInvertedMode(out position, out ray);
            if (!mouseIsOver)
                return;

            if (_rotationEnabled) {
                if (leftMouseButtonClick) {
#if VR_GOOGLE
																				mouseDragStartScreenPos = GvrController.TouchPos;
#endif
                    mouseDragStartLocalPosition = transform.InverseTransformPoint(position);
                    mouseStartedDragging = true;
                    hasDragged = false;
                    clickStart = Time.time;
                } else if (mouseStartedDragging && (leftMouseButtonPressed || (Input.touchSupported && Input.touchCount == 1))) {
#if VR_GOOGLE
																				float distFactor = Mathf.Min (Vector3.Distance (_cameraMain.transform.position, transform.position) / transform.localScale.y, 1f);
																				Vector3 dragDirection = (mouseDragStartScreenPos - (Vector3)GvrController.TouchPos) * distFactor * _mouseDragSensitivity;
																				dragDirection.y *= -1.0f;
																				if (dragDirection.x != 0 || dragDirection.y != 0) {
																				hasDragged = true;
																				gameObject.transform.Rotate (Vector3.up, dragDirection.x, Space.World);
																				Vector3 axisY = Vector3.Cross (transform.position - _cameraMain.transform.position, Vector3.up);
																				transform.Rotate (axisY, dragDirection.y, Space.World);																				}
#else
                    Vector3 localPos = transform.InverseTransformPoint(position);
                    if (localPos != mouseDragStartLocalPosition) {
                        float angle = Vector3.Angle(mouseDragStartLocalPosition, localPos);
                        Quaternion rot = Quaternion.AngleAxis(angle, Vector3.Cross(mouseDragStartLocalPosition, localPos));
                        if (_rotationSpeed < 1f) {
                            Quaternion newRot = transform.rotation * rot;
                            transform.rotation = Quaternion.Slerp(transform.rotation, newRot, _rotationSpeed);
                        } else {
                            transform.rotation *= rot;
                        }
                        if (angle > _dragThreshold)
                            hasDragged = true;
                    }
#endif
                } else {
                    mouseStartedDragging = false;
                }
            }

            if (_rightClickRotates && rightMouseButtonPressed) {
                Vector3 axis = _cameraMain.transform.forward;
                float rotAngle = _rightClickRotatingClockwise ? -2f : 2f;
                if (Input.GetKey(KeyCode.LeftAlt)) {
                    rotAngle *= -1;
                }
                transform.Rotate(axis, rotAngle, Space.World);
            }

            if (leftMouseButtonRelease && !hasDragged && (Time.time - clickStart <= _clickDuration) && OnTileClick != null) {
                OnTileClick(lastHoverTileIndex);
                lastClickedTile = lastHoverTileIndex;
            }

            if (_zoomEnabled) {
                // Use mouse wheel to zoom in and out
                float wheel = Input.GetAxis("Mouse ScrollWheel");
                wheelAccel += wheel;

                // Support for pinch on mobile
                if (Input.touchSupported && Input.touchCount == 2) {
                    // Store both touches.
                    Touch touchZero = Input.GetTouch(0);
                    Touch touchOne = Input.GetTouch(1);

                    // Find the position in the previous frame of each touch.
                    Vector2 touchZeroPrevPos = touchZero.position - touchZero.deltaPosition;
                    Vector2 touchOnePrevPos = touchOne.position - touchOne.deltaPosition;

                    // Find the magnitude of the vector (the distance) between the touches in each frame.
                    float prevTouchDeltaMag = (touchZeroPrevPos - touchOnePrevPos).magnitude;
                    float touchDeltaMag = (touchZero.position - touchOne.position).magnitude;

                    // Find the difference in the distances between each frame.
                    float deltaMagnitudeDiff = prevTouchDeltaMag - touchDeltaMag;

                    // Pass the delta to the wheel accel
                    wheelAccel += deltaMagnitudeDiff;
                }

                if (wheelAccel != 0) {
                    wheelAccel = Mathf.Clamp(wheelAccel, -0.1f, 0.1f);
                    if (wheelAccel >= 0.01f || wheelAccel <= -0.01f) {
                        _cameraMain.fieldOfView = Mathf.Clamp(_cameraMain.fieldOfView + (90.0f * _cameraMain.fieldOfView / MAX_FIELD_OF_VIEW) * wheelAccel * _zoomSpeed, MIN_FIELD_OF_VIEW, MAX_FIELD_OF_VIEW);
                        wheelAccel *= _zoomDamping; // smooth dampening
                    }
                } else {
                    wheelAccel = 0;
                }
            }
        }


        void CheckMousePosNormalMode(out Vector3 position, out Ray ray) {
            mouseIsOver = GetHitPoint(out position, out ray);
            if (!mouseIsOver)
                return;

            if (_highlightEnabled || OnTileClick != null || OnTileMouseOver != null || (!Application.isPlaying && useEditorRay)) {
                int tileIndex;
                if (_extruded && _raycast3D) {
                    tileIndex = GetTileInRayDirection(ray, position);
                } else {
                    Vector3 localPosition = transform.InverseTransformPoint(position);
                    tileIndex = GetTileAtLocalPosition(localPosition);
                }
                if (tileIndex >= 0 && tileIndex != lastHighlightedTileIndex) {
                    lastHoverTileIndex = tileIndex;
                    if (OnTileMouseOver != null)
                        OnTileMouseOver(tileIndex);
                    if (_highlightEnabled) {
                        if (lastHighlightedTile != null)
                            HideHighlightedTile();
                        lastHighlightedTile = tiles[tileIndex];
                        lastHighlightedTileIndex = tileIndex;
                        SetTileMaterial(lastHighlightedTileIndex, highlightMaterial, true);
                    }
                } else if (tileIndex < 0 && lastHighlightedTileIndex >= 0) {
                    HideHighlightedTile();
                }
            }
        }

        void CheckMousePosInvertedMode(out Vector3 position, out Ray ray) {
            mouseIsOver = GetHitPoint(out position, out ray);
            if (!mouseIsOver)
                return;

            if (_highlightEnabled || OnTileClick != null || OnTileMouseOver != null) {
                int tileIndex;
                Vector3 localPosition = transform.InverseTransformPoint(position);
                tileIndex = GetTileAtLocalPosition(localPosition);
                if (tileIndex >= 0 && tileIndex != lastHighlightedTileIndex) {
                    lastHoverTileIndex = tileIndex;
                    if (OnTileMouseOver != null)
                        OnTileMouseOver(tileIndex);
                    if (_highlightEnabled) {
                        if (lastHighlightedTile != null)
                            HideHighlightedTile();
                        lastHighlightedTile = tiles[tileIndex];
                        lastHighlightedTileIndex = tileIndex;
                        SetTileMaterial(lastHighlightedTileIndex, highlightMaterial, true);
                    }
                } else if (tileIndex < 0 && lastHighlightedTileIndex >= 0) {
                    HideHighlightedTile();
                }
            }
        }


        #endregion

        #region Hexasphere builder

        // internal fields
        const string HEXASPHERE_WIREFRAME = "WireFrame";
        const string HEXASPHERE_SHADEDFRAME = "ShadedFrame";
        const string HEXASPHERE_SHADEDFRAME_GO = "Shade";
        const string HEXASPHERE_TILESROOT = "TilesRoot";
        const int HEXASPHERE_MAX_PARTS = 100;
        const int MAX_VERTEX_COUNT_PER_CHUNK = 65500;
        const int VERTEX_ARRAY_SIZE = 65530;

        Dictionary<Point, Point> points = new Dictionary<Point, Point>();
        Dictionary<Point, int> verticesIdx = new Dictionary<Point, int>();
        List<Vector3>[] verticesWire = new List<Vector3>[HEXASPHERE_MAX_PARTS];
        List<int>[] indicesWire = new List<int>[HEXASPHERE_MAX_PARTS];
        List<Vector2>[] uvWire = new List<Vector2>[HEXASPHERE_MAX_PARTS];
        List<Color32>[] colorWire = new List<Color32>[HEXASPHERE_MAX_PARTS];
        List<Vector3>[] verticesShaded = new List<Vector3>[HEXASPHERE_MAX_PARTS];
        List<int>[] indicesShaded = new List<int>[HEXASPHERE_MAX_PARTS];
        List<Vector4>[] uvShaded = new List<Vector4>[HEXASPHERE_MAX_PARTS];
        List<Vector4>[] uv2Shaded = new List<Vector4>[HEXASPHERE_MAX_PARTS];
        List<Color32>[] colorShaded = new List<Color32>[HEXASPHERE_MAX_PARTS];
        const float PHI = 1.61803399f;
        List<Texture2D> texArray = new List<Texture2D>(255);
        Dictionary<Color, Texture2D> solidTexCache = new Dictionary<Color, Texture2D>();
        Mesh[] shadedMeshes = new Mesh[HEXASPHERE_MAX_PARTS];
        MeshFilter[] shadedMFs = new MeshFilter[HEXASPHERE_MAX_PARTS];
        MeshRenderer[] shadedMRs = new MeshRenderer[HEXASPHERE_MAX_PARTS];
        Mesh[] wiredMeshes = new Mesh[HEXASPHERE_MAX_PARTS];
        MeshFilter[] wiredMFs = new MeshFilter[HEXASPHERE_MAX_PARTS];
        MeshRenderer[] wiredMRs = new MeshRenderer[HEXASPHERE_MAX_PARTS];
        bool[] colorShadedDirty = new bool[HEXASPHERE_MAX_PARTS];
        bool[] uvShadedDirty = new bool[HEXASPHERE_MAX_PARTS];
        bool[] uvWireDirty = new bool[HEXASPHERE_MAX_PARTS];
        bool[] colorWireDirty = new bool[HEXASPHERE_MAX_PARTS];
        [SerializeField] Vector3 oldCameraPosition;

        Material gridMatExtrusion {
            get {
                if (_gridMatExtrusion == null) {
                    _gridMatExtrusion = new Material(Shader.Find("Hexasphere/HexaGridExtrusion"));
                    _gridMatExtrusion.hideFlags = HideFlags.DontSave;
                }
                return _gridMatExtrusion;
            }
        }

        Material gridMatNoExtrusion {
            get {
                if (_gridMatNoExtrusion == null) {
                    _gridMatNoExtrusion = Instantiate(Resources.Load<Material>("Materials/HexaGridMatNoExtrusion")) as Material;
                    _gridMatNoExtrusion.hideFlags = HideFlags.DontSave;
                }
                return _gridMatNoExtrusion;
            }
        }

        Material tileShadedFrameMatBevel {
            get {
                if (_tileShadedFrameMatBevel == null) {
                    _tileShadedFrameMatBevel = new Material(Shader.Find("Hexasphere/HexaTileBackgroundBevel"));
                    _tileShadedFrameMatBevel.hideFlags = HideFlags.DontSave;
                }
                return _tileShadedFrameMatBevel;
            }
        }


        Material tileShadedFrameMatExtrusion {
            get {
                if (_tileShadedFrameMatExtrusion == null) {
                    _tileShadedFrameMatExtrusion = new Material(Shader.Find("Hexasphere/HexaTileBackgroundExtrusion"));
                    _tileShadedFrameMatExtrusion.hideFlags = HideFlags.DontSave;
                }
                return _tileShadedFrameMatExtrusion;
            }
        }

        Material tileShadedFrameMatNoExtrusion {
            get {
                if (_tileShadedFrameMatNoExtrusion == null) {
                    _tileShadedFrameMatNoExtrusion = Instantiate(Resources.Load<Material>("Materials/HexaTilesBackgroundMatNoExtrusion")) as Material;
                    _tileShadedFrameMatNoExtrusion.hideFlags = HideFlags.DontSave;
                }
                return _tileShadedFrameMatNoExtrusion;
            }
        }

        Material tileColoredMat {
            get {
                if (_tileColoredMat == null) {
                    _tileColoredMat = Instantiate(Resources.Load<Material>("Materials/HexaTilesMat")) as Material;
                    _tileColoredMat.hideFlags = HideFlags.DontSave;
                }
                return _tileColoredMat;
            }
        }

        Material tileTexturedMat {
            get {
                if (_tileTexturedMat == null) {
                    _tileTexturedMat = Instantiate(Resources.Load<Material>("Materials/HexaTilesTexturedMat")) as Material;
                    _tileTexturedMat.hideFlags = HideFlags.DontSave;
                }
                return _tileTexturedMat;
            }
        }



        Point GetCachedPoint(Point point) {
            Point thePoint;
            if (points.TryGetValue(point, out thePoint)) {
                return thePoint;
            } else {
                points[point] = point;
                return point;
            }
        }

        /// <summary>
        /// Updates shader properties and generate hexasphere geometry if divisions or style has changed
        /// </summary>
        public void UpdateMaterialProperties() {

            shouldUpdateMaterialProperties = false;

            _numDivisions = Mathf.Max(1, _numDivisions);
            _tileTextureSize = Mathf.Max(32, (int)Mathf.Pow(2, (int)Mathf.Log(_tileTextureSize, 2)));
            if (highlightMaterial != null) {
                highlightMaterial.color = _highlightColor;
            }

            // In inverted mode, moves the camera into the center of the sphere
            if (_cameraMain != null && currentInvertedMode != _invertedMode) {
                if (_invertedMode) {
                    oldCameraPosition = _cameraMain.transform.position;
                    _cameraMain.transform.position = transform.position;
                } else {
                    _cameraMain.transform.position = oldCameraPosition;
                }
            }

            if (tiles == null || currentDivisions != _numDivisions || currentStyle != _style || currentTextureSize != _tileTextureSize || currentDefaultShadedColor != _defaultShadedColor || currentExtruded != _extruded || _rotationShift != currentRotationShift || _invertedMode != currentInvertedMode || currentBevel != _bevel) {
                Generate();
            } else {
                if (needRegenerate || needRegenerateWireframe || currentWireframeColorFromTile != _wireframeColorFromTile || currentSmartEdges != _smartEdges) {
                    RebuildWireframe();
                }
                if (needRegenerate) {
                    needRegenerate = false;
                    RebuildTiles();
                }
                UpdateShadedMaterials();
                UpdateMeshRenderersShadowSupport();
            }
            if (_tileShadedFrameMatExtrusion != null) {
                _tileShadedFrameMatExtrusion.SetFloat("_GradientIntensity", 1f - _gradientIntensity);
                _tileShadedFrameMatExtrusion.SetFloat("_ExtrusionMultiplier", _extrudeMultiplier);
                _tileShadedFrameMatExtrusion.SetColor("_Color", _tileTintColor);
                _tileShadedFrameMatExtrusion.SetColor("_AmbientColor", _ambientColor);
                _tileShadedFrameMatExtrusion.SetFloat("_MinimumLight", _minimumLight);
            }
            if (_tileShadedFrameMatBevel != null) {
                _tileShadedFrameMatBevel.SetFloat("_GradientIntensity", 1f - _gradientIntensity);
                _tileShadedFrameMatBevel.SetFloat("_ExtrusionMultiplier", _extrudeMultiplier);
                _tileShadedFrameMatBevel.SetColor("_Color", _tileTintColor);
                _tileShadedFrameMatBevel.SetColor("_AmbientColor", _ambientColor);
                _tileShadedFrameMatBevel.SetFloat("_MinimumLight", _minimumLight);
            }
            if (_tileShadedFrameMatNoExtrusion != null) {
                _tileShadedFrameMatNoExtrusion.SetColor("_Color", _tileTintColor);
                _tileShadedFrameMatNoExtrusion.SetColor("_AmbientColor", _ambientColor);
                _tileShadedFrameMatNoExtrusion.SetFloat("_MinimumLight", _minimumLight);
                _tileShadedFrameMatNoExtrusion.SetColor("_SpecularTint", _specularTint);
                _tileShadedFrameMatNoExtrusion.SetFloat("_Smoothness", _smoothness * 100f);
            }
            UpdateLightingMode();

            Color wireColor = _wireframeColor;
            wireColor.r *= _wireframeIntensity;
            wireColor.g *= _wireframeIntensity;
            wireColor.b *= _wireframeIntensity;
            if (_gridMatExtrusion != null) {
                _gridMatExtrusion.SetFloat("_GradientIntensity", 1f - _gradientIntensity);
                _gridMatExtrusion.SetFloat("_ExtrusionMultiplier", _extrudeMultiplier);
                _gridMatExtrusion.SetColor("_Color", wireColor);
            }
            if (_gridMatNoExtrusion != null) {
                _gridMatNoExtrusion.SetColor("_Color", wireColor);
            }
            sphereCollider.radius = _extruded ? 0.5f * (1.0f + _extrudeMultiplier) : 0.5f;

            UpdateTransparencyMode();
            if (_transparencyTiles != currentTransparencyTiles) {
                UpdateTilesTransparency();
            }

            UpdateBevel();

            FixedUpdate();
        }

        void UpdateTilesTransparency() {
            currentTransparencyTiles = _transparencyTiles;
            foreach (KeyValuePair<Color, Material> kvp in colorCache) {
                Material mat = kvp.Value;
                if (mat != null) {
                    mat.SetFloat(ShaderParams.TileAlpha, _transparencyTiles);
                }
            }
            foreach (KeyValuePair<int, Material> kvp in textureCache) {
                Material mat = kvp.Value;
                if (mat != null) {
                    mat.SetFloat(ShaderParams.TileAlpha, _transparencyTiles);
                }
            }
        }


        void UpdateTransparencyMode() {
            UpdateTransparencyMat(_tileShadedFrameMatBevel);
            UpdateTransparencyMat(_tileShadedFrameMatExtrusion);
            UpdateTransparencyMat(_tileShadedFrameMatNoExtrusion);
            UpdateTransparencyMat(_gridMatExtrusion);
            UpdateTransparencyMat(_gridMatNoExtrusion);
            UpdateTransparencyMat(_tileColoredMat);
            UpdateTransparencyMat(_tileTexturedMat);

            if (_tileShadedFrameMatExtrusion != null) {
                if (_transparencyCull) {
                    _tileShadedFrameMatExtrusion.EnableKeyword("HEXA_ALPHA");
                } else {
                    _tileShadedFrameMatExtrusion.DisableKeyword("HEXA_ALPHA");
                }
            }
            if (_tileShadedFrameMatBevel != null) {
                if (_transparencyCull) {
                    _tileShadedFrameMatBevel.EnableKeyword("HEXA_ALPHA");
                } else {
                    _tileShadedFrameMatBevel.DisableKeyword("HEXA_ALPHA");
                }
            }
        }

        void UpdateLightingMode() {
            if (_lighting) {
                Shader.EnableKeyword("HEXA_LIT");
            } else {
                if (Shader.IsKeywordEnabled("HEXA_LIT")) {
                    Shader.DisableKeyword("HEXA_LIT");
                }
            }
        }

        void UpdateTransparencyMat(Material mat) {
            if (mat == null)
                return;
            if (_transparent) {
                mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
				mat.SetInt ("_ZWrite", _transparencyZWrite ? 1 : 0);
                if (mat.renderQueue < 3000) {
                    mat.renderQueue += 2000;
                }
                mat.SetInt("_Cull", _transparencyDoubleSided ? (int)UnityEngine.Rendering.CullMode.Off : (int)UnityEngine.Rendering.CullMode.Back);
            } else {
                mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
                mat.SetInt("_ZWrite", 1);
                if (mat.renderQueue >= 3000) {
                    mat.renderQueue -= 2000;
                }
                mat.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Back);
            }
            mat.SetFloat(ShaderParams.TileAlpha, _transparencyTiles);
        }

        void UpdateMeshRenderersShadowSupport() {

            MeshRenderer[] rr = transform.GetComponentsInChildren<MeshRenderer>(true);
            for (int k = 0; k < rr.Length; k++) {
                if (rr[k] != null && rr[k].name.Equals(HEXASPHERE_SHADEDFRAME_GO)) {
                    rr[k].receiveShadows = _receiveShadows;
                    if (_castShadows) {
                        rr[k].shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
                    } else {
                        rr[k].shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                    }
                }
            }
        }

        /// <summary>
        /// Generate the hexasphere geometry.
        /// </summary>
        public void Generate() {
#if TRACE_PERFORMANCE
			DateTime dt = DateTime.Now;
#endif

            Point[] corners = new Point[] {
                new Point (1, PHI, 0),
                new Point (-1, PHI, 0),
                new Point (1, -PHI, 0),
                new Point (-1, -PHI, 0),
                new Point (0, 1, PHI),
                new Point (0, -1, PHI),
                new Point (0, 1, -PHI),
                new Point (0, -1, -PHI),
                new Point (PHI, 0, 1),
                new Point (-PHI, 0, 1),
                new Point (PHI, 0, -1),
                new Point (-PHI, 0, -1)
            };

            if (_rotationShift != Misc.Vector3zero) {
                Quaternion q = Quaternion.Euler(_rotationShift);
                for (int k = 0; k < corners.Length; k++) {
                    Point c = corners[k];
                    Vector3 v = (Vector3)c;
                    v = q * v;
                    c.x = v.x;
                    c.y = v.y;
                    c.z = v.z;
                }
            }


            Triangle[] triangles = new Triangle[] {
                new Triangle (corners [0], corners [1], corners [4], false),
                new Triangle (corners [1], corners [9], corners [4], false),
                new Triangle (corners [4], corners [9], corners [5], false),
                new Triangle (corners [5], corners [9], corners [3], false),
                new Triangle (corners [2], corners [3], corners [7], false),
                new Triangle (corners [3], corners [2], corners [5], false),
                new Triangle (corners [7], corners [10], corners [2], false),
                new Triangle (corners [0], corners [8], corners [10], false),
                new Triangle (corners [0], corners [4], corners [8], false),
                new Triangle (corners [8], corners [2], corners [10], false),
                new Triangle (corners [8], corners [4], corners [5], false),
                new Triangle (corners [8], corners [5], corners [2], false),
                new Triangle (corners [1], corners [0], corners [6], false),
                new Triangle (corners [11], corners [1], corners [6], false),
                new Triangle (corners [3], corners [9], corners [11], false),
                new Triangle (corners [6], corners [10], corners [7], false),
                new Triangle (corners [3], corners [11], corners [7], false),
                new Triangle (corners [11], corners [6], corners [7], false),
                new Triangle (corners [6], corners [0], corners [10], false),
                new Triangle (corners [9], corners [1], corners [11], false)
            };


            DestroyCachedTiles(false);

            currentDivisions = _numDivisions;
            currentStyle = _style;
            currentExtruded = _extruded;
            currentBevel = _bevel;
            currentDefaultShadedColor = _defaultShadedColor;
            currentRotationShift = _rotationShift;
            currentInvertedMode = _invertedMode;

            points.Clear();

            for (int i = 0; i < corners.Length; i++) {
                points[corners[i]] = corners[i];
            }

#if TRACE_PERFORMANCE
			Debug.Log ("Stage 1 " + DateTime.Now);
#endif

            List<Point> bottom = new List<Point>();
            int triCount = triangles.Length;
            for (int f = 0; f < triCount; f++) {
                List<Point> prev = null;
                Point point0 = triangles[f].points[0];
                bottom.Clear();
                bottom.Add(point0);
                List<Point> left = point0.Subdivide(triangles[f].points[1], numDivisions, GetCachedPoint);
                List<Point> right = point0.Subdivide(triangles[f].points[2], numDivisions, GetCachedPoint);
                for (int i = 1; i <= numDivisions; i++) {
                    prev = bottom;
                    bottom = left[i].Subdivide(right[i], i, GetCachedPoint);
                    new Triangle(prev[0], bottom[0], bottom[1]);
                    for (int j = 1; j < i; j++) {
                        new Triangle(prev[j], bottom[j], bottom[j + 1]);
                        new Triangle(prev[j - 1], prev[j], bottom[j]);
                    }
                }
            }

#if TRACE_PERFORMANCE
		Debug.Log ("Stage 2 " + DateTime.Now);
#endif
            int meshPointsCount = points.Values.Count;

#if TRACE_PERFORMANCE
			Debug.Log ("Stage 2.1 " + DateTime.Now);
#endif

#if TRACE_PERFORMANCE
			Debug.Log ("Stage 2.2 " + DateTime.Now);
#endif
            int p = 0;
            Point.flag = 0;
            tiles = new Tile[meshPointsCount];
            foreach (Point point in points.Values) {
                tiles[p] = new Tile(point, p);
                p++;
            }
#if TRACE_PERFORMANCE
			Debug.Log ("Stage 3 " + DateTime.Now);
#endif
            // Check metal
            if (_extruded && SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.Metal) {
                Debug.LogWarning("Extrusion uses geometry shaders which are not supported on Metal. Please consider switching to OpenGL Graphics API in Player Settings.");
            }

            lastHitTileIndex = -1;
            lastHighlightedTile = null;
            lastHighlightedTileIndex = -1;
            lastClickedTile = -1;

            // Destroy placeholders
            Transform t = gameObject.transform.Find(HEXASPHERE_WIREFRAME);
            if (t != null)
                DestroyImmediate(t.gameObject);
            t = gameObject.transform.Find(HEXASPHERE_SHADEDFRAME);
            if (t != null)
                DestroyImmediate(t.gameObject);
            t = gameObject.transform.Find(HEXASPHERE_TILESROOT);
            if (t != null)
                DestroyImmediate(t.gameObject);

            // Create meshes
            BuildWireframe();
            BuildTiles();

            UpdateMeshRenderersShadowSupport();

#if TRACE_PERFORMANCE
			Debug.Log ("Stage 3.1 " + DateTime.Now);
#endif

            needRefreshRouteMatrix = true;

#if TRACE_PERFORMANCE
			Debug.Log ("Stage 4 " + DateTime.Now);
			Debug.Log ("Time = " + (DateTime.Now - dt).TotalSeconds + " s.");
#endif
        }

        List<T> CheckList<T>(ref List<T> l) {
            if (l == null) {
                l = new List<T>(VERTEX_ARRAY_SIZE);
            } else {
                l.Clear();
            }
            return l;
        }


        void RebuildWireframe() {
            Transform t = gameObject.transform.Find(HEXASPHERE_WIREFRAME);
            if (t != null)
                DestroyImmediate(t.gameObject);
            BuildWireframe();
        }

        void BuildWireframe() {

            if (_style == STYLE.Shaded) {
                return;
            }

            currentWireframeColorFromTile = _wireframeColorFromTile;
            currentSmartEdges = _smartEdges;
            needRegenerateWireframe = false;

            // Check which sides are borders
            if (_smartEdges) {
                for (int k = 0; k < tiles.Length; k++) {
                    Tile tile = tiles[k];
                    tile.borders = 63;
                    for (int v = 0; v < tile.vertices.Length; v++) {
                        Vector3 p0 = tile.vertices[v];
                        Vector3 p1 = v < tile.vertices.Length - 1 ? tile.vertices[v + 1] : tile.vertices[0];
                        for (int n = 0; n < tile.neighbours.Length; n++) {
                            Tile neighbour = tile.neighbours[n];
                            if (neighbour.customMat == tile.customMat) {
                                for (int w = 0; w < neighbour.vertices.Length; w++) {
                                    Vector3 q0 = neighbour.vertices[w];
                                    Vector3 q1 = w < neighbour.vertices.Length - 1 ? neighbour.vertices[w + 1] : neighbour.vertices[0];
                                    if (p0 == q0 && p1 == q1 || p0 == q1 && p1 == q0) {
                                        tile.borders &= 63 - (1 << v);
                                        n = 9999;
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }
            }


            if (_extruded && !_invertedMode) {
                BuildWireframeExtruded();
                return;
            }

            int chunkIndex = 0;
            List<Vector3> vertexChunk = CheckList<Vector3>(ref verticesWire[chunkIndex]);
            List<int> indicesChunk = CheckList<int>(ref indicesWire[chunkIndex]);

            int pos;
            int verticesCount = -1;
            verticesIdx.Clear();
            int tileCount = tiles.Length;
            if (_smartEdges) {
                for (int k = 0; k < tileCount; k++) {
                    if (verticesCount > MAX_VERTEX_COUNT_PER_CHUNK) {
                        chunkIndex++;
                        vertexChunk = CheckList<Vector3>(ref verticesWire[chunkIndex]);
                        indicesChunk = CheckList<int>(ref indicesWire[chunkIndex]);
                        verticesIdx.Clear();
                        verticesCount = -1;
                    }
                    Tile tile = tiles[k];
                    if (!tile.visible)
                        continue;
                    Point[] tileVertices = tile.vertexPoints;
                    int tileVerticesCount = tileVertices.Length;
                    bool vertexRequired = false;
                    for (int b = 0; b <= tileVerticesCount; b++) {
                        bool segmentVisible = (tile.borders & (1 << b)) != 0;
                        if (segmentVisible || vertexRequired) {
                            int vertexIndex = b < tileVerticesCount ? b : 0;
                            Point point = tileVertices[vertexIndex];
                            if (!verticesIdx.TryGetValue(point, out pos)) {
                                vertexChunk.Add(point.projectedVector3);
                                verticesCount++;
                                pos = verticesCount;
                                verticesIdx[point] = pos;
                            }
                            if (vertexRequired) {
                                indicesChunk.Add(pos); // close previous segment
                            }
                            if (segmentVisible && b < tileVerticesCount) { // starts a new segment
                                indicesChunk.Add(pos);
                            }
                            vertexRequired = segmentVisible;
                        }
                    }
                }
            } else {
                int pos0 = 0;
                for (int k = 0; k < tileCount; k++) {
                    if (verticesCount > MAX_VERTEX_COUNT_PER_CHUNK) {
                        chunkIndex++;
                        vertexChunk = CheckList<Vector3>(ref verticesWire[chunkIndex]);
                        indicesChunk = CheckList<int>(ref indicesWire[chunkIndex]);
                        verticesIdx.Clear();
                        verticesCount = -1;
                    }
                    Tile tile = tiles[k];
                    if (!tile.visible)
                        continue;
                    Point[] tileVertices = tile.vertexPoints;
                    int tileVerticesCount = tileVertices.Length;
                    for (int b = 0; b < tileVerticesCount; b++) {
                        Point point = tileVertices[b];
                        if (!verticesIdx.TryGetValue(point, out pos)) {
                            vertexChunk.Add(point.projectedVector3);
                            verticesCount++;
                            pos = verticesCount;
                            verticesIdx[point] = pos;
                        }
                        indicesChunk.Add(pos);
                        if (b == 0) {
                            pos0 = pos;
                        } else {
                            indicesChunk.Add(pos);
                        }
                    }
                    indicesChunk.Add(pos0);
                }
            }

            GameObject partsRoot = CreateGOandParent(gameObject.transform, HEXASPHERE_WIREFRAME);
            for (int k = 0; k <= chunkIndex; k++) {
                GameObject go = CreateGOandParent(partsRoot.transform, "Wire");
                MeshFilter mf = go.AddComponent<MeshFilter>();
                wiredMeshes[k] = new Mesh();
                wiredMeshes[k].hideFlags = HideFlags.DontSave;
                wiredMeshes[k].SetVertices(verticesWire[k]);
                wiredMeshes[k].SetIndices(indicesWire[k].ToArray(), MeshTopology.Lines, 0, false);
                mf.sharedMesh = wiredMeshes[k];
                wiredMFs[k] = mf;
                MeshRenderer mr = go.AddComponent<MeshRenderer>();
                wiredMRs[k] = mr;
                mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                mr.sharedMaterial = gridMatNoExtrusion;
            }

        }

        void BuildWireframeExtruded() {

            int chunkIndex = 0;
            List<Vector3> vertexChunk = CheckList<Vector3>(ref verticesWire[chunkIndex]);
            List<Vector2> uvChunk = CheckList<Vector2>(ref uvWire[chunkIndex]);
            List<Color32> colorChunk = CheckList<Color32>(ref colorWire[chunkIndex]);
            List<int> indicesChunk = CheckList<int>(ref indicesWire[chunkIndex]);

            int verticesCount = 0;
            int tileCount = tiles.Length;
            Vector2 uvExtrudeData;

            if (_smartEdges) {
                for (int k = 0; k < tileCount; k++) {
                    Tile tile = tiles[k];
                    if (!tile.visible)
                        continue;
                    if (verticesCount > MAX_VERTEX_COUNT_PER_CHUNK) {
                        chunkIndex++;
                        vertexChunk = CheckList<Vector3>(ref verticesWire[chunkIndex]);
                        uvChunk = CheckList<Vector2>(ref uvWire[chunkIndex]);
                        colorChunk = CheckList<Color32>(ref colorWire[chunkIndex]);
                        indicesChunk = CheckList<int>(ref indicesWire[chunkIndex]);
                        verticesCount = 0;
                    }
                    Point[] tileVertices = tile.vertexPoints;
                    int tileVerticesCount = tileVertices.Length;
                    uvExtrudeData.x = k;
                    uvExtrudeData.y = tile.extrudeAmount;
                    tile.uvWireChunkIndex = chunkIndex;
                    tile.uvWireChunkStart = verticesCount;
                    tile.uvWireChunkLength = 0;
                    Color32 tileColor;
                    if (_wireframeColorFromTile && tile.customMat != null) {
                        tileColor = tile.customMat.color;
                    } else {
                        tileColor = Misc.Color32White;
                    }

                    int pos0 = verticesCount;
                    bool vertexRequired = false;
                    bool vertex0Missing = true;
                    for (int b = 0; b < tileVerticesCount; b++) {
                        bool segmentVisible = (tile.borders & (1 << b)) != 0;
                        if (segmentVisible || vertexRequired) {
                            Point point = tileVertices[b];
                            vertexChunk.Add(point.projectedVector3);
                            uvChunk.Add(uvExtrudeData);
                            colorChunk.Add(tileColor);
                            if (vertexRequired) {
                                indicesChunk.Add(verticesCount);  // close previous segment
                            }
                            if (segmentVisible) { // starts a new segment
                                indicesChunk.Add(verticesCount);
                                if (b == 0) {
                                    vertex0Missing = false;
                                }
                            }
                            verticesCount++;
                            tile.uvWireChunkLength++;
                        }
                        vertexRequired = segmentVisible;
                    }
                    if (vertexRequired) {
                        if (vertex0Missing) {
                            Point point = tileVertices[0];
                            vertexChunk.Add(point.projectedVector3);
                            uvChunk.Add(uvExtrudeData);
                            colorChunk.Add(tileColor);
                            indicesChunk.Add(verticesCount);
                            verticesCount++;
                        } else {
                            indicesChunk.Add(pos0);
                        }
                    }
                }
            } else {
                for (int k = 0; k < tileCount; k++) {
                    Tile tile = tiles[k];
                    if (!tile.visible)
                        continue;
                    if (verticesCount > MAX_VERTEX_COUNT_PER_CHUNK) {
                        chunkIndex++;
                        vertexChunk = CheckList<Vector3>(ref verticesWire[chunkIndex]);
                        uvChunk = CheckList<Vector2>(ref uvWire[chunkIndex]);
                        colorChunk = CheckList<Color32>(ref colorWire[chunkIndex]);
                        indicesChunk = CheckList<int>(ref indicesWire[chunkIndex]);
                        verticesCount = 0;
                    }
                    int pos0 = verticesCount;
                    Point[] tileVertices = tile.vertexPoints;
                    int tileVerticesCount = tileVertices.Length;
                    uvExtrudeData.x = k;
                    uvExtrudeData.y = tile.extrudeAmount;
                    tile.uvWireChunkIndex = chunkIndex;
                    tile.uvWireChunkStart = verticesCount;
                    tile.uvWireChunkLength = tileVerticesCount;
                    Color32 tileColor;
                    if (_wireframeColorFromTile && tile.customMat != null) {
                        tileColor = tile.customMat.color;
                    } else {
                        tileColor = Misc.Color32White;
                    }
                    for (int b = 0; b < tileVerticesCount; b++) {
                        Point point = tileVertices[b];
                        vertexChunk.Add(point.projectedVector3);
                        uvChunk.Add(uvExtrudeData);
                        colorChunk.Add(tileColor);
                        indicesChunk.Add(verticesCount);
                        if (b > 0) {
                            indicesChunk.Add(verticesCount);
                        }
                        verticesCount++;
                    }
                    indicesChunk.Add(pos0);
                }
            }



            GameObject partsRoot = CreateGOandParent(gameObject.transform, HEXASPHERE_WIREFRAME);
            for (int k = 0; k <= chunkIndex; k++) {
                uvWireDirty[k] = false;
                colorWireDirty[k] = false;
                GameObject go = CreateGOandParent(partsRoot.transform, "Wire");
                MeshFilter mf = go.AddComponent<MeshFilter>();
                wiredMeshes[k] = new Mesh();
                wiredMeshes[k].hideFlags = HideFlags.DontSave;
                wiredMeshes[k].SetVertices(verticesWire[k]);
                wiredMeshes[k].SetUVs(0, uvWire[k]);
                wiredMeshes[k].SetColors(colorWire[k]);
                wiredMeshes[k].SetIndices(indicesWire[k].ToArray(), MeshTopology.Lines, 0, false);
                mf.sharedMesh = wiredMeshes[k];
                wiredMFs[k] = mf;
                MeshRenderer mr = go.AddComponent<MeshRenderer>();
                mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                mr.sharedMaterial = gridMatExtrusion;
                wiredMRs[k] = mr;
            }
            wireChunkCount = chunkIndex + 1;
        }

        void UpdateWireMaterialsFast() {
            if (_style == STYLE.Shaded || !_extruded || _invertedMode)
                return;

            for (int k = 0; k < wireChunkCount; k++) {
                if (uvWireDirty[k]) {
                    uvWireDirty[k] = false;
                    wiredMeshes[k].SetUVs(0, uvWire[k]);
                }
                if (colorWireDirty[k]) {
                    colorWireDirty[k] = false;
                    wiredMeshes[k].SetColors(colorWire[k]);
                }
            }
        }


        void RebuildTiles() {
            Transform t = gameObject.transform.Find(HEXASPHERE_SHADEDFRAME);
            if (t != null)
                DestroyImmediate(t.gameObject);
            t = gameObject.transform.Find(HEXASPHERE_TILESROOT);
            if (t != null)
                DestroyImmediate(t.gameObject);
            BuildTiles();
        }

        void BuildTiles() {

            if (tiles == null || _style == STYLE.Wireframe)
                return;

            int chunkIndex = 0;
            List<Vector3> vertexChunk = CheckList<Vector3>(ref verticesShaded[chunkIndex]);
            List<int> indexChunk = CheckList<int>(ref indicesShaded[chunkIndex]);

            bool useGPos = _extruded && (_bevel || _lighting || _transparencyCull);
            List<Vector4> uv2Chunk = useGPos ? CheckList<Vector4>(ref uv2Shaded[chunkIndex]) : null;

            int verticesCount = 0;
            int tileCount = tiles.Length;
            int[] hexIndices, pentIndices;
            if (_invertedMode) {
                hexIndices = hexagonIndicesInverted;
                pentIndices = pentagonIndicesInverted;
            } else {
                if (_extruded) {
                    hexIndices = hexagonIndicesExtruded;
                    pentIndices = pentagonIndicesExtruded;
                } else {
                    hexIndices = hexagonIndices;
                    pentIndices = pentagonIndices;
                }
            }
            for (int k = 0; k < tileCount; k++) {
                Tile tile = tiles[k];
                if (!tile.visible)
                    continue;
                if (verticesCount > MAX_VERTEX_COUNT_PER_CHUNK) {
                    chunkIndex++;
                    vertexChunk = CheckList<Vector3>(ref verticesShaded[chunkIndex]);
                    indexChunk = CheckList<int>(ref indicesShaded[chunkIndex]);
                    if (useGPos) {
                        uv2Chunk = CheckList<Vector4>(ref uv2Shaded[chunkIndex]);
                    }
                    verticesCount = 0;
                }
                Point[] tileVertices = tile.vertexPoints;
                int tileVerticesCount = tileVertices.Length;
                Vector4 gpos = Misc.Vector4zero;
                for (int b = 0; b < tileVerticesCount; b++) {
                    Point point = tileVertices[b];
                    Vector3 v = point.projectedVector3;
                    vertexChunk.Add(v);
                    gpos.x += v.x;
                    gpos.y += v.y;
                    gpos.z += v.z;
                }
                gpos.x /= tileVerticesCount;
                gpos.y /= tileVerticesCount;
                gpos.z /= tileVerticesCount;
                int[] indicesList;
                if (tileVerticesCount == 6) {
                    if (_extruded) {
                        vertexChunk.Add((tileVertices[1].projectedVector3 + tileVertices[5].projectedVector3) * 0.5f);
                        vertexChunk.Add((tileVertices[2].projectedVector3 + tileVertices[4].projectedVector3) * 0.5f);
                        tileVerticesCount += 2;
                    }
                    indicesList = hexIndices;
                } else {
                    if (_extruded) {
                        vertexChunk.Add((tileVertices[1].projectedVector3 + tileVertices[4].projectedVector3) * 0.5f);
                        vertexChunk.Add((tileVertices[2].projectedVector3 + tileVertices[4].projectedVector3) * 0.5f);
                        tileVerticesCount += 2;
                        gpos.w = 1.0f; // cancel bevel effect on pentagons
                    }
                    indicesList = pentIndices;
                }
                if (useGPos) {
                    for (int b = 0; b < tileVerticesCount; b++) {
                        uv2Chunk.Add(gpos);
                    }
                }
                for (int b = 0; b < indicesList.Length; b++) {
                    indexChunk.Add(verticesCount + indicesList[b]);
                }
                verticesCount += tileVerticesCount;
            }

            Material tileShadedFrameMat = GetShadedFrameMat();
            GameObject partsRoot = CreateGOandParent(gameObject.transform, HEXASPHERE_SHADEDFRAME);
            for (int k = 0; k <= chunkIndex; k++) {
                GameObject go = CreateGOandParent(partsRoot.transform, HEXASPHERE_SHADEDFRAME_GO);
                MeshFilter mf = go.AddComponent<MeshFilter>();
                shadedMFs[k] = mf;
                if (shadedMeshes[k] == null) {
                    shadedMeshes[k] = new Mesh();
                    shadedMeshes[k].hideFlags = HideFlags.DontSave;
                }
                shadedMeshes[k].Clear();
                shadedMeshes[k].SetVertices(verticesShaded[k]);
                shadedMeshes[k].SetTriangles(indicesShaded[k], 0); // SetIndices (indicesShaded [k].ToArray (), MeshTopology.Triangles, 0, false);
                if (useGPos) {
                    shadedMeshes[k].SetUVs(1, uv2Shaded[k]);
                }
                mf.sharedMesh = shadedMeshes[k];
                MeshRenderer mr = go.AddComponent<MeshRenderer>();
                shadedMRs[k] = mr;
                mr.sharedMaterial = tileShadedFrameMat;
            }

            BuildShadedMaterials();
        }

        Material GetShadedFrameMat() {
            if (_extruded) {
                if (_bevel) {
                    return tileShadedFrameMatBevel;
                } else {
                    return tileShadedFrameMatExtrusion;
                }
            } else {
                return tileShadedFrameMatNoExtrusion;
            }
        }

        void BuildShadedMaterials() {

            if (tiles == null || _style == STYLE.Wireframe)
                return;

            int chunkIndex = 0;
            List<Vector4> uvChunk = CheckList(ref uvShaded[chunkIndex]);
            List<Color32> colorChunk = CheckList(ref colorShaded[chunkIndex]);
            Material tileShadedFrameMat = GetShadedFrameMat();
            if (whiteTex == null)
                whiteTex = GetCachedSolidTex(Color.white);
            texArray.Clear();
            texArray.Add(whiteTex);

            int verticesCount = 0;
            int tileCount = tiles.Length;
            Color32 defaultShaderColor32 = _defaultShadedColor;

            for (int k = 0; k < tileCount; k++) {
                Tile tile = tiles[k];
                if (!tile.visible)
                    continue;
                if (verticesCount > MAX_VERTEX_COUNT_PER_CHUNK) {
                    chunkIndex++;
                    uvChunk = CheckList(ref uvShaded[chunkIndex]);
                    colorChunk = CheckList(ref colorShaded[chunkIndex]);
                    verticesCount = 0;
                }
                Point[] tileVertices = tile.vertexPoints;
                int tileVerticesCount = tileVertices.Length;
                Vector2[] uvList;
                if (tileVerticesCount == 6) {
                    if (currentExtruded) {
                        uvList = hexagonUVsExtruded;
                    } else {
                        uvList = _invertedMode ? hexagonUVsInverted : hexagonUVs;
                    }
                } else {
                    if (currentExtruded) {
                        uvList = pentagonUVsExtruded;
                    } else {
                        uvList = _invertedMode ? pentagonUVsInverted : pentagonUVs;
                    }
                }
                // Put tile color or texture into tex array
                Texture2D tileTexture;
                int textureIndex = 0;
                Vector2 textureScale, textureOffset;
                if (tile.customMat && tile.customMat.HasProperty(ShaderParams.MainTex) && tile.customMat.mainTexture != null) {
                    tileTexture = (Texture2D)tile.customMat.mainTexture;
                    textureIndex = texArray.IndexOf(tileTexture);
                    textureScale = tile.customMat.mainTextureScale;
                    textureOffset = tile.customMat.mainTextureOffset;
                } else {
                    tileTexture = whiteTex;
                    textureScale = Misc.Vector2one;
                    textureOffset = Misc.Vector2zero;
                }
                if (textureIndex < 0) {
                    texArray.Add(tileTexture);
                    textureIndex = texArray.Count - 1;
                }
                Color32 color;
                if (tile.customMat != null) {
                    color = tile.customMat.color;
                } else {
                    color = defaultShaderColor32;
                }
                Vector4 uv4;
                tile.uvShadedChunkStart = verticesCount;
                tile.uvShadedChunkIndex = chunkIndex;
                tile.uvShadedChunkLength = uvList.Length;

                float cosTheta = 0;
                float sinTheta = 0;
                if (tile.rotation != 0) {
                    cosTheta = Mathf.Cos(tile.rotation);
                    sinTheta = Mathf.Sin(tile.rotation);
                }
                for (int b = 0; b < uvList.Length; b++) {
                    Vector2 uv = uvList[b];
                    float x = uv.x;
                    float y = uv.y;
                    if (tile.rotation != 0) {
                        RotateUV(ref x, ref y, cosTheta, sinTheta);
                    }
                    uv4.x = x * textureScale.x + textureOffset.x;
                    uv4.y = y * textureScale.y + textureOffset.y;
                    uv4.z = textureIndex;
                    uv4.w = tile.extrudeAmount;
                    uvChunk.Add(uv4);
                    colorChunk.Add(color);
                }
                verticesCount += uvList.Length;
            }

            for (int k = 0; k <= chunkIndex; k++) {
                uvShadedDirty[k] = false;
                colorShadedDirty[k] = false;
                shadedMeshes[k].SetUVs(0, uvShaded[k]);
                shadedMeshes[k].SetColors(colorShaded[k]);
                shadedMFs[k].sharedMesh = shadedMeshes[k];
                shadedMRs[k].sharedMaterial = tileShadedFrameMat;
            }

            // Build texture array
            if (allowedTextureArray) {
                int texArrayCount = texArray.Count;
                currentTextureSize = _tileTextureSize;
                Texture2DArray finalTexArray = new Texture2DArray(_tileTextureSize, _tileTextureSize, texArrayCount, TextureFormat.ARGB32, true);
                for (int k = 0; k < texArrayCount; k++) {
                    if (texArray[k].width != _tileTextureSize || texArray[k].height != _tileTextureSize) {
                        texArray[k] = Instantiate(texArray[k]);
                        texArray[k].hideFlags = HideFlags.DontSave;
                        TextureScaler.Scale(texArray[k], _tileTextureSize, _tileTextureSize, FilterMode.Trilinear);
                    }
                    finalTexArray.SetPixels32(texArray[k].GetPixels32(), k);
                }
                finalTexArray.Apply(true, true);
                tileShadedFrameMat.SetTexture(ShaderParams.MainTex, finalTexArray);
            }

            pendingTextureArrayUpdate = false;
            pendingColorsUpdate = false;
            pendingUVUpdateFast = false;
            uvChunkCount = chunkIndex + 1;
        }

        void UpdateShadedMaterials() {

            if (tiles == null || _style == STYLE.Wireframe)
                return;

            int chunkIndex = 0;
            List<Vector4> uvChunk = uvShaded[chunkIndex];
            List<Color32> colorChunk = colorShaded[chunkIndex];
            Material tileShadedFrameMat = GetShadedFrameMat();
            if (whiteTex == null)
                whiteTex = GetCachedSolidTex(Color.white);
            texArray.Clear();
            texArray.Add(whiteTex);

            int verticesCount = 0;
            int tileCount = tiles.Length;
            Color color = _defaultShadedColor;
            for (int k = 0; k < tileCount; k++) {
                Tile tile = tiles[k];
                if (!tile.visible)
                    continue;
                if (verticesCount > MAX_VERTEX_COUNT_PER_CHUNK) {
                    chunkIndex++;
                    uvChunk = uvShaded[chunkIndex];
                    colorChunk = colorShaded[chunkIndex];
                    verticesCount = 0;
                }
                Point[] tileVertices = tile.vertexPoints;
                int tileVerticesCount = tileVertices.Length;
                Vector2[] uvList;
                if (tileVerticesCount == 6) {
                    if (currentExtruded) {
                        uvList = hexagonUVsExtruded;
                    } else {
                        uvList = _invertedMode ? hexagonUVsInverted : hexagonUVs;
                    }
                } else {
                    if (currentExtruded) {
                        uvList = pentagonUVsExtruded;
                    } else {
                        uvList = _invertedMode ? pentagonUVsInverted : pentagonUVs;
                    }
                }
                // Put tile color or texture into tex array
                Texture2D tileTexture;
                int textureIndex = 0;
                Vector2 textureScale, textureOffset;
                if (tile.customMat && tile.customMat.HasProperty(ShaderParams.MainTex) && tile.customMat.mainTexture != null) {
                    tileTexture = (Texture2D)tile.customMat.mainTexture;
                    textureIndex = texArray.IndexOf(tileTexture);
                    textureScale = tile.customMat.mainTextureScale;
                    textureOffset = tile.customMat.mainTextureOffset;
                } else {
                    tileTexture = whiteTex;
                    textureScale = Misc.Vector2one;
                    textureOffset = Misc.Vector2zero;
                }
                if (textureIndex < 0) {
                    texArray.Add(tileTexture);
                    textureIndex = texArray.Count - 1;
                }
                if (pendingColorsUpdate) {
                    color = tile.customMat ? tile.customMat.color : _defaultShadedColor;
                }
                float cosTheta = 0;
                float sinTheta = 0;

                if (tile.rotation != 0) {
                    cosTheta = Mathf.Cos(tile.rotation);
                    sinTheta = Mathf.Sin(tile.rotation);
                }
                for (int b = 0; b < uvList.Length; b++) {
                    Vector2 uv = uvList[b];
                    float x = uv.x;
                    float y = uv.y;
                    if (tile.rotation != 0) {
                        RotateUV(ref x, ref y, cosTheta, sinTheta);
                    }
                    Vector4 uv4;
                    uv4.x = x * textureScale.x + textureOffset.x;
                    uv4.y = y * textureScale.y + textureOffset.y;
                    uv4.z = textureIndex;
                    uv4.w = tile.extrudeAmount;
                    uvChunk[verticesCount] = uv4;
                    if (pendingColorsUpdate) {
                        colorChunk[verticesCount] = color;
                    }
                    verticesCount++;
                }
            }

            for (int k = 0; k <= chunkIndex; k++) {
                uvShadedDirty[k] = false;
                shadedMeshes[k].SetUVs(0, uvShaded[k]);
                if (pendingColorsUpdate) {
                    colorShadedDirty[k] = false;
                    shadedMeshes[k].SetColors(colorShaded[k]);
                }
                shadedMFs[k].sharedMesh = shadedMeshes[k];
                shadedMRs[k].sharedMaterial = tileShadedFrameMat;
            }

            if (pendingTextureArrayUpdate && allowedTextureArray) {
                // Build texture array
                int texArrayCount = texArray.Count;
                currentTextureSize = _tileTextureSize;
                Texture2DArray finalTexArray = new Texture2DArray(_tileTextureSize, _tileTextureSize, texArrayCount, TextureFormat.ARGB32, true);
                for (int k = 0; k < texArrayCount; k++) {
                    if (texArray[k].width != _tileTextureSize || texArray[k].height != _tileTextureSize) {
#if UNITY_EDITOR
                        string path = AssetDatabase.GetAssetPath(texArray[k]);
                        if (!string.IsNullOrEmpty(path)) {
                            TextureImporter imp = (TextureImporter)TextureImporter.GetAtPath(path);
                            if (imp.textureCompression != TextureImporterCompression.Uncompressed) {
                                Debug.LogError("Texture is compressed. Please change its import settings to an uncompressed format.");
                                continue;
                            }
                            if (!imp.isReadable) {
                                Debug.LogError("Texture is not marked as readable. Please change its import settings.");
                                continue;
                            }
                        }
#endif
                        texArray[k] = Instantiate(texArray[k]) as Texture2D;
                        texArray[k].hideFlags = HideFlags.DontSave;
                        TextureScaler.Scale(texArray[k], _tileTextureSize, _tileTextureSize, FilterMode.Trilinear);
                    }
                    finalTexArray.SetPixels32(texArray[k].GetPixels32(), k);
                }
                finalTexArray.Apply(true, true);
                tileShadedFrameMat.SetTexture("_MainTex", finalTexArray);
                pendingTextureArrayUpdate = false;
            }
            pendingColorsUpdate = false;
        }

        void RotateUV(ref float x, ref float y, float cosTheta, float sinTheta) {
            x -= 0.5f;
            y -= 0.5f;
            float x1 = cosTheta * x - sinTheta * y;
            float y1 = sinTheta * x + cosTheta * y;
            x = x1 + 0.5f;
            y = y1 + 0.5f;
        }


        void UpdateShadedMaterialsFast() {

            if (_style == STYLE.Wireframe)
                return;

            if (pendingColorsUpdate) {
                for (int k = 0; k < uvChunkCount; k++) {
                    if (colorShadedDirty[k]) {
                        colorShadedDirty[k] = false;
                        shadedMeshes[k].SetColors(colorShaded[k]);
                    }
                }
                pendingColorsUpdate = false;
            }

            if (pendingUVUpdateFast) {
                for (int k = 0; k < uvChunkCount; k++) {
                    if (uvShadedDirty[k]) {
                        uvShadedDirty[k] = false;
                        shadedMeshes[k].SetUVs(0, uvShaded[k]);
                    }
                }
            }
        }

        Texture2D GetCachedSolidTex(Color color) {
            Texture2D tex;
            if (solidTexCache.TryGetValue(color, out tex)) {
                return tex;
            } else {
                tex = new Texture2D(_tileTextureSize, _tileTextureSize, TextureFormat.ARGB32, true);
                tex.hideFlags = HideFlags.DontSave;
                int l = tex.width * tex.height;
                Color32[] colors32 = new Color32[l];
                Color32 color32 = color;
                for (int k = 0; k < l; k++) {
                    colors32[k] = color32;
                }
                tex.SetPixels32(colors32);
                tex.Apply();
                solidTexCache[color] = tex;
                return tex;
            }
        }



        GameObject CreateGOandParent(Transform parent, string name) {
            GameObject go = new GameObject(name);
            go.layer = parent.gameObject.layer;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = Misc.Vector3zero;
            go.transform.localScale = Vector3.one;
            go.transform.localRotation = Quaternion.Euler(0, 0, 0);
            return go;
        }

        #endregion

        #region Tile functions

        Transform tilesRoot;
        int[] hexagonIndices = new int[] {
            0, 1, 5,
            1, 2, 5,
            4, 5, 2,
            3, 4, 2
        };
        Vector2[] hexagonUVs = new Vector2[] {
            new Vector2 (0, 0.5f),
            new Vector2 (0.25f, 1f),
            new Vector2 (0.75f, 1f),
            new Vector2 (1f, 0.5f),
            new Vector2 (0.75f, 0f),
            new Vector2 (0.25f, 0f)
        };
        Vector2[] hexagonUVsInverted = new Vector2[] {	// same but y' = 1 - y
			new Vector2 (0, 0.5f),
            new Vector2 (0.25f, 0f),
            new Vector2 (0.75f, 0f),
            new Vector2 (1f, 0.5f),
            new Vector2 (0.75f, 1f),
            new Vector2 (0.25f, 1f)
        };
        int[] hexagonIndicesExtruded = new int[] {
            0, 1, 6,
            5, 0, 6,
            1, 2, 5,
            4, 5, 2,
            2, 3, 7,
            3, 4, 7
        };
        Vector2[] hexagonUVsExtruded = new Vector2[] {
            new Vector2 (0, 0.5f),
            new Vector2 (0.25f, 1f),
            new Vector2 (0.75f, 1f),
            new Vector2 (1f, 0.5f),
            new Vector2 (0.75f, 0f),
            new Vector2 (0.25f, 0f),
            new Vector2 (0.25f, 0.5f),
            new Vector2 (0.75f, 0.5f)
        };
        int[] hexagonIndicesInverted = new int[] {
            0, 5, 1,
            1, 5, 2,
            4, 2, 5,
            3, 2, 4
        };

        int[] pentagonIndices = new int[] {
            0, 1, 4,
            1, 2, 4,
            3, 4, 2
        };
        Vector2[] pentagonUVs = new Vector2[] {
            new Vector2 (0, 0.33f),
            new Vector2 (0.25f, 1f),
            new Vector2 (0.75f, 1f),
            new Vector2 (1f, 0.33f),
            new Vector2 (0.5f, 0f),
        };
        Vector2[] pentagonUVsInverted = new Vector2[] { // same but y' = 1 - y
			new Vector2 (0f, 0.66f),
            new Vector2 (0.25f, 0f),
            new Vector2 (0.75f, 0f),
            new Vector2 (1f, 0.66f),
            new Vector2 (0.5f, 1f),
        };
        int[] pentagonIndicesExtruded = new int[] {
            0, 1, 5,
            4, 0, 5,
            1, 2, 4,
            2, 3, 6,
            3, 4, 6
        };
        Vector2[] pentagonUVsExtruded = new Vector2[] {
            new Vector2 (0, 0.33f),
            new Vector2 (0.25f, 1f),
            new Vector2 (0.75f, 1f),
            new Vector2 (1f, 0.33f),
            new Vector2 (0.5f, 0f),
            new Vector2 (0.375f, 0.5f),
            new Vector2 (0.625f, 0.5f)

        };
        int[] pentagonIndicesInverted = new int[] {
            0, 4, 1,
            1, 4, 2,
            3, 2, 4
        };
        Dictionary<Color, Material> colorCache = new Dictionary<Color, Material>();
        Dictionary<int, Material> textureCache = new Dictionary<int, Material>();


        void UpdateTileMeshVertexPositions(int tileIndex) {
            Tile tile = tiles[tileIndex];
            if (tile.renderer == null) return;
            MeshFilter mf = tile.renderer.GetComponent<MeshFilter>();
            Mesh mesh = mf.sharedMesh;
            if (mesh == null) return;
            if (_extruded) {
                Vector3[] tileVertices = tile.vertices;
                Vector3[] extrudedVertices = new Vector3[tileVertices.Length];
                for (int k = 0; k < tileVertices.Length; k++) {
                    extrudedVertices[k] = tileVertices[k] * (1f + tile.extrudeAmount * _extrudeMultiplier);
                }
                mesh.vertices = extrudedVertices;
            } else {
                mesh.vertices = tile.vertices;
            }
            mesh.normals = tile.vertices;
            mf.sharedMesh = null;
            mf.sharedMesh = mesh;
        }


        void GenerateTileMesh(int tileIndex, Material mat) {
            if (tilesRoot == null) {
                tilesRoot = CreateGOandParent(gameObject.transform, HEXASPHERE_TILESROOT).transform;
            }
            GameObject go = CreateGOandParent(tilesRoot, "Tile");
            MeshFilter mf = go.AddComponent<MeshFilter>();
            Mesh mesh = new Mesh();
            mesh.hideFlags = HideFlags.DontSave;
            Tile tile = tiles[tileIndex];
            if (_extruded) {
                Vector3[] tileVertices = tile.vertices;
                Vector3[] extrudedVertices = new Vector3[tileVertices.Length];
                for (int k = 0; k < tileVertices.Length; k++) {
                    extrudedVertices[k] = tileVertices[k] * (1f + tile.extrudeAmount * _extrudeMultiplier);
                }
                mesh.vertices = extrudedVertices;
            } else {
                mesh.vertices = tile.vertices;
            }
            mesh.normals = tile.vertices;
            int tileVerticesCount = tile.vertices.Length;
            Vector2[] uv;
            if (tileVerticesCount == 6) {
                mesh.SetIndices(_invertedMode ? hexagonIndicesInverted : hexagonIndices, MeshTopology.Triangles, 0, false);
                uv = _invertedMode ? hexagonUVsInverted : hexagonUVs;
            } else {
                mesh.SetIndices(_invertedMode ? pentagonIndicesInverted : pentagonIndices, MeshTopology.Triangles, 0, false);
                uv = _invertedMode ? pentagonUVsInverted : pentagonUVs;
            }
            if (tile.rotation != 0) {
                Vector2[] oldUV = uv;
                uv = new Vector2[uv.Length];
                for (int k = 0; k < uv.Length; k++) {   // copy array to avoid modification of the source in case of rotation
                    uv[k] = oldUV[k];
                }
                float cosTheta = Mathf.Cos(tile.rotation);
                float sinTheta = Mathf.Sin(tile.rotation);
                for (int b = 0; b < uv.Length; b++) {
                    RotateUV(ref uv[b].x, ref uv[b].y, cosTheta, sinTheta);
                }
            }
            mesh.uv = uv;

            mf.sharedMesh = mesh;
            MeshRenderer mr = go.AddComponent<MeshRenderer>();
            mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            mr.sharedMaterial = mat;
            tile.renderer = mr;
        }

        int CombineHash(int hash1, int hash2) {
            unchecked {
                int hash = 17;
                hash = hash * 31 + hash1;
                hash = hash * 31 + hash2;
                return hash;
            }
        }

        Material GetCachedMaterial(Color color, Texture2D texture = null) {
            Material mat;
            if (texture == null) {
                if (colorCache.TryGetValue(color, out mat)) {
                    return mat;
                }
                mat = Instantiate(tileColoredMat);
                colorCache[color] = mat;
            } else {
                int key = CombineHash(texture.GetHashCode(), color.GetHashCode());
                if (textureCache.TryGetValue(key, out mat)) {
                    return mat;
                }
                mat = Instantiate(tileTexturedMat);
                mat.mainTexture = texture;
                textureCache[key] = mat;
            }
            mat.hideFlags = HideFlags.DontSave;
            mat.color = color;
            mat.SetFloat(ShaderParams.TileAlpha, _transparencyTiles);
            return mat;
        }


        void HideHighlightedTile() {
            if (lastHighlightedTileIndex >= 0 && lastHighlightedTile != null && lastHighlightedTile.renderer != null && lastHighlightedTile.renderer.sharedMaterial == highlightMaterial) {
                if (lastHighlightedTile.tempMat != null) {
                    TileRestoreTemporaryMaterial(lastHighlightedTileIndex);
                } else if (tiles[lastHighlightedTileIndex].renderer != null) {
                    tiles[lastHighlightedTileIndex].renderer.enabled = false;
                }
            }
            ResetHighlightMaterial();
            lastHighlightedTile = null;
            lastHighlightedTileIndex = -1;
        }


        void TileRestoreTemporaryMaterial(int tileIndex) {
            if (tileIndex < 0 || tileIndex >= tiles.Length)
                return;
            Tile tile = tiles[tileIndex];
            if (tile.tempMat != null) {
                tile.renderer.sharedMaterial = tile.tempMat;
            }
        }

        void ResetHighlightMaterial() {
            if (highlightMaterial != null) {
                Color co = highlightMaterial.color;
                co.a = 0.2f;
                highlightMaterial.SetColor(ShaderParams.Color2, co);
                if (highlightMaterial.HasProperty(ShaderParams.MainTex)) {
                    highlightMaterial.mainTexture = null;
                }
            }
        }

        void RefreshHighlightedTile() {
            if (lastHighlightedTileIndex < 0 || lastHighlightedTileIndex >= tiles.Length)
                return;
            SetTileMaterial(lastHighlightedTileIndex, highlightMaterial, true);
        }


        void DestroyCachedTiles(bool preserveMaterials) {
            if (tiles == null)
                return;

            HideHighlightedTile();
            for (int k = 0; k < tiles.Length; k++) {
                Tile tile = tiles[k];
                if (tile.renderer != null) {
                    DestroyImmediate(tile.renderer.gameObject);
                    tile.renderer = null;
                    if (!preserveMaterials) {
                        tile.customMat = null;
                        tile.tempMat = null;
                    }
                }
            }

        }

        Tile GetNearestTileToPosition(Tile[] tiles, Vector3 localPosition, out float distance) {
            distance = float.MaxValue;
            Tile nearest = null;
            for (int k = 0; k < tiles.Length; k++) {
                Tile tile = tiles[k];
                Vector3 center = tile.center;
                // unwrapped SqrMagnitude for performance considerations
                float dist = (center.x - localPosition.x) * (center.x - localPosition.x) + (center.y - localPosition.y) * (center.y - localPosition.y) + (center.z - localPosition.z) * (center.z - localPosition.z);
                if (dist < distance) {
                    nearest = tile;
                    distance = dist;
                }
            }
            return nearest;
        }


        int GetTileAtLocalPosition(Vector3 localPosition) {

            if (tiles == null)
                return -1;

            // If this the same tile? Heuristic: any neighour will be farther
            if (lastHitTileIndex >= 0 && lastHitTileIndex < tiles.Length) {
                Tile lastHitTile = tiles[lastHitTileIndex];
                if (lastHitTile != null) {
                    float dist = Vector3.SqrMagnitude(lastHitTile.center - localPosition);
                    bool valid = true;
                    for (int k = 0; k < lastHitTile.neighbours.Length; k++) {
                        float otherDist = Vector3.SqrMagnitude(lastHitTile.neighbours[k].center - localPosition);
                        if (otherDist < dist) {
                            valid = false;
                            break;
                        }
                    }
                    if (valid) {
                        return lastHitTileIndex;
                    }
                }
            } else {
                lastHitTileIndex = 0;
            }

            // follow the shortest path to the minimum distance
            Tile nearest = tiles[lastHitTileIndex];
            float tileDist;
            float minDist = 1e6f;
            for (int k = 0; k < tiles.Length; k++) {
                Tile newNearest = GetNearestTileToPosition(nearest.neighbours, localPosition, out tileDist);
                if (tileDist < minDist) {
                    minDist = tileDist;
                    nearest = newNearest;
                } else {
                    break;
                }
            }
            lastHitTileIndex = nearest.index;
            return lastHitTileIndex;
        }

#if RAYCAST3D_DEBUG
								bool rayDebug;
								void PutBall (Vector3 pos, Color color) {
												GameObject obj = GameObject.CreatePrimitive (PrimitiveType.Sphere);
												obj.transform.position = pos;
												obj.transform.localScale = Vector3.one * 0.1f;
												obj.GetComponent<Renderer> ().material.color = color;
								}
#endif

        int GetTileInRayDirection(Ray ray, Vector3 worldPosition) {
            if (tiles == null)
                return -1;

            // Compute final point
            Vector3 minPoint = worldPosition;
            Vector3 maxPoint = worldPosition + ray.direction * transform.localScale.x * 0.5f;
            float rangeMin = transform.localScale.x * 0.5f;
            rangeMin *= rangeMin;
            float rangeMax = (worldPosition - transform.position).sqrMagnitude;
            float dist;
            Vector3 bestPoint = maxPoint;
            for (int k = 0; k < 10; k++) {
                Vector3 midPoint = (minPoint + maxPoint) * 0.5f;
                dist = (midPoint - transform.position).sqrMagnitude;
                if (dist < rangeMin) {
                    maxPoint = midPoint;
                    bestPoint = midPoint;
                } else if (dist > rangeMax) {
                    maxPoint = midPoint;
                } else {
                    minPoint = midPoint;
                }
            }

            // Get tile at first hit
            int nearest = GetTileAtLocalPosition(transform.InverseTransformPoint(worldPosition));
            if (nearest < 0)
                return -1;

            Vector3 currPoint = worldPosition;
            Tile tile = tiles[nearest];
            Vector3 tileTop = transform.TransformPoint(tile.center * (1.0f + tile.extrudeAmount * _extrudeMultiplier));
            float tileHeight = (tileTop - transform.position).sqrMagnitude;
            float rayHeight = (currPoint - transform.position).sqrMagnitude;
            float minDist = 1e6f;
            dist = minDist;
            const int NUM_STEPS = 10;
            int candidate = -1;
            for (int k = 1; k <= NUM_STEPS; k++) {
                dist = Mathf.Abs(rayHeight - tileHeight);
                if (dist < minDist) {
                    minDist = dist;
                    candidate = nearest;

                }
                if (rayHeight < tileHeight) {
#if RAYCAST3D_DEBUG
																				rayDebug = false;
#endif
                    return candidate;
                }
                float t = k / (float)NUM_STEPS;
                currPoint = worldPosition * (1f - t) + bestPoint * t;
#if RAYCAST3D_DEBUG
																if (rayDebug)
																				PutBall (currPoint, Color.red);
#endif

                nearest = GetTileAtLocalPosition(transform.InverseTransformPoint(currPoint));
                if (nearest < 0)
                    break;
                tile = tiles[nearest];
                tileTop = transform.TransformPoint(tile.center * (1.0f + tile.extrudeAmount * _extrudeMultiplier));
#if RAYCAST3D_DEBUG
																if (rayDebug)
																				PutBall (tileTop, Color.blue);
#endif
                tileHeight = (tileTop - transform.position).sqrMagnitude;
                rayHeight = (currPoint - transform.position).sqrMagnitude;
            }

#if RAYCAST3D_DEBUG
												rayDebug = false;
#endif
            if (dist < minDist) {
                minDist = dist;
                candidate = nearest;

            }
            if (rayHeight < tileHeight) {
                return candidate;
            } else {
                return -1;
            }
        }


        #endregion

        #region Raycasting functions - separated here so they can be modified to fit other purposes

#if VR_GOOGLE
		Transform GVR_Reticle;
		bool GVR_TouchStarted;
#endif
#if VR_SAMSUNG_GEAR_CONTROLLER
        LineRenderer SVR_Laser;
#endif
        float lastTimeCheckVRPointers;

        void RegisterVRPointers() {
            if (Time.time - lastTimeCheckVRPointers < 1f)
                return;
            lastTimeCheckVRPointers = Time.time;

#if VR_GOOGLE
												GameObject obj = GameObject.Find ("GvrControllerPointer");
												if (obj != null) {
												Transform t = obj.transform.Find ("Laser");
												if (t != null) {
												GVR_Reticle = t.FindChild ("Reticle");
												}
												}
#elif VR_SAMSUNG_GEAR_CONTROLLER
												GameObject obj = GameObject.Find ("GearVrController");
												if (obj != null) {
			Transform t = obj.transform.Find ("Model/Laser");
												if (t != null) {
												SVR_Laser = t.gameObject.GetComponent<LineRenderer>();
												}
												}
#endif
        }

        bool GetHitPoint(out Vector3 position, out Ray ray) {
            RaycastHit hit;
            ray = GetRay();
            if (_invertedMode) {
                if (Physics.Raycast(ray.origin + ray.direction * transform.localScale.z, -ray.direction, out hit, transform.localScale.z)) {
                    if (hit.collider.gameObject == gameObject) {
                        position = hit.point;
                        return true;
                    }
                }
            } else {
                if (Physics.Raycast(ray, out hit)) {
                    if (hit.collider.gameObject == gameObject) {
                        position = hit.point;
                        return true;
                    }
                }
            }

            position = Misc.Vector3zero;
            return false;
        }

        Ray GetRay() {
            Ray ray;

            if (useEditorRay && !Application.isPlaying) {
                return editorRay;
            }

            if (_VREnabled) {
#if VR_GOOGLE
																if (GVR_Reticle != null && GVR_Reticle.gameObject.activeInHierarchy) {
																Vector3 screenPoint = _cameraMain.WorldToScreenPoint (GVR_Reticle.position);
																ray = _cameraMain.ScreenPointToRay (screenPoint);
																} else {
																RegisterVRPointers();
																ray = new Ray (_cameraMain.transform.position, GvrController.Orientation * Vector3.forward);
																}
#elif VR_SAMSUNG_GEAR_CONTROLLER && UNITY_5_5_OR_NEWER
																if (SVR_Laser != null && SVR_Laser.gameObject.activeInHierarchy) {
																Vector3 endPos = SVR_Laser.GetPosition(1);
																if (!SVR_Laser.useWorldSpace) endPos = SVR_Laser.transform.TransformPoint(endPos);
																Vector3 screenPoint = _cameraMain.WorldToScreenPoint (endPos);
																ray = _cameraMain.ScreenPointToRay (screenPoint);
																} else {
																RegisterVRPointers();
																ray = new Ray (_cameraMain.transform.position, _cameraMain.transform.rotation * Vector3.forward);
																}
#else
                ray = new Ray(_cameraMain.transform.position, _cameraMain.transform.forward);
#endif
            } else {
                Vector3 mousePos = Input.mousePosition;
                ray = _cameraMain.ScreenPointToRay(mousePos);
            }
            return ray;
        }

        #endregion

        #region Bevel support

        void UpdateBevel() {
            if (!_bevel)
                return;

            if (_tileShadedFrameMatBevel != null) {
                GenerateBevenNormalsTexture();
                _tileShadedFrameMatBevel.SetTexture("_BumpMask", bevelNormals);
            }
        }


        void GenerateBevenNormalsTexture() {

            const int texSize = 256;
            if (bevelNormals == null || bevelNormals.width != texSize) {
                bevelNormals = new Texture2D(texSize, texSize, TextureFormat.ARGB32, false);
            }

            int th = bevelNormals.height;
            int tw = bevelNormals.width;
            if (bevelNormalsColors == null || bevelNormalsColors.Length != th * tw) {
                bevelNormalsColors = new Color[th * tw];
            }

            int index = 0;
            Vector2 pt;
            const float bevelWidth = 0.1f;
            float bevelWidthSqr = bevelWidth * bevelWidth;
            for (int y = 0; y < th; y++) {
                pt.y = (float)y / th;
                for (int x = 0; x < tw; x++) {
                    pt.x = (float)x / tw;
                    bevelNormalsColors[index].r = 0f;
                    float minDistSqr = float.MaxValue;
                    for (int t = 0; t < 6; t++) {
                        Vector2 t0 = hexagonUVsExtruded[t];
                        Vector2 t1 = t < 5 ? hexagonUVsExtruded[t + 1] : hexagonUVsExtruded[0];
                        float distSqr = SqrDistanceToSegment(t0, t1, pt);
                        if (distSqr < minDistSqr) {
                            minDistSqr = distSqr;
                        }
                    }
                    float f = minDistSqr / bevelWidthSqr;
                    if (f > 1f)
                        f = 1f;
                    bevelNormalsColors[index].r = f;
                    index++;
                }
            }
            bevelNormals.SetPixels(bevelNormalsColors);
            bevelNormals.Apply();
        }

        float SqrDistanceToSegment(Vector2 v, Vector2 w, Vector2 p) {
            float l2 = Vector2.SqrMagnitude(v - w);
            float t = Mathf.Max(0, Mathf.Min(1, Vector2.Dot(p - v, w - v) / l2));
            Vector2 projection = v + t * (w - v);
            return Vector2.SqrMagnitude(p - projection);
        }

        #endregion

        #region Misc functions

        List<T> GetTempList<T>(ref List<T> list) {
            if (list == null) {
                list = new List<T>(64);
            } else {
                list.Clear();
            }
            return list;
        }

        Dictionary<P, Q> GetTempDictionary<P, Q>(ref Dictionary<P, Q> dict) {
            if (dict == null) {
                dict = new Dictionary<P, Q>(64);
            } else {
                dict.Clear();
            }
            return dict;
        }

        #endregion



    }

}