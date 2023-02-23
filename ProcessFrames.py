import os, sys, getopt, cv2

def Frames2Features(inputfolder, outputfolder, filenameformat):
    # Remove older version of the processed video if it exists
    if (os.path.exists(outputfolder)):
        os.rmtree(outputfolder)
    os.makedirs(outputfolder)
    
    files = os.listdir(inputfolder)
    
    # Validate frames
    for i, file in enumerate(files):
        if (file != filenameformat.replace("{0}", str(i))):
            print("Invalid file found:", file, "expected:", filenameformat.replace("{0}", str(i)))
            sys.exit()

    grey_scale_images = []
    for file in files:
        img = cv2.imread(filenameformat.replace("{0}", str(i)))
        gray = cv2.cvtColor(img, cv2.COLOR_BGR2GRAY)
        grey_scale_images.append(gray)

    sift = cv2.xfeatures2d.SIFT_create()
    
    keypoints = []
    descriptors = []
    for image in grey_scale_images:
        kp, des = sift.detectAndCompute(image, None)
        keypoints.append(kp)
        descriptors.append(des)
    
    #TODO write features to csv

if __name__ == "__main__":
    inputfolder = ''
    outputfolder = ''
    opts, args = getopt.getopt(sys.argv[1:], "hi:o:", ["input=", "output="])
    for opt, arg in opts:
        if opt == '-h':
            print ('python ProcessFrames.py -i <inputfolder> -o <outputfolder>')
            sys.exit()
        elif opt in ("-i"):
            inputfolder = arg
        elif opt in ("-o"):
            outputfolder = arg
    print('Input folder is', inputfolder)
    print('Output folder is', outputfolder)
    Frames2Features(inputfolder, outputfolder, "frame{0}.jpg")