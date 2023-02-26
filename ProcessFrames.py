import os, sys, getopt, cv2, shutil, PickleUtil

def Frames2Features(inputfolder, outputfolder, filenameformat):
    # Remove older version of the processed video if it exists
    if (os.path.exists(outputfolder)):
        shutil.rmtree(outputfolder)
    os.makedirs(outputfolder)
    
    files = os.listdir(inputfolder)
    
    files_hashset = set(files)
    
    # Validate frames
    for i, file in enumerate(files):
        if (filenameformat.replace("{0}", str(i)) not in files_hashset):
            print("Invalid file found:", file, "expected:", filenameformat.replace("{0}", str(i)))
            sys.exit()

    # Generate features 
    sift = cv2.SIFT_create()
    
    for i, file in enumerate(files):
        img = cv2.imread(os.path.join(inputfolder, filenameformat.replace("{0}", str(i))))
        gray = cv2.cvtColor(img, cv2.COLOR_BGR2GRAY)
        
        tmpkeys, desc = sift.detectAndCompute(gray, None)
        
        image=cv2.drawKeypoints(gray,tmpkeys,img,flags=cv2.DRAW_MATCHES_FLAGS_DRAW_RICH_KEYPOINTS)
        
        keyp = [(point.pt, point.size, point.angle, point.response, point.octave, point.class_id, desc) for point in tmpkeys]
        
        # Save the frame as a JPEG image file
        filename = os.path.join(outputfolder, file)
        cv2.imwrite(filename, image)
        
        PickleUtil.Save(os.path.join(outputfolder, file.replace(".jpg", ".keyp")), keyp)
        PickleUtil.Save(os.path.join(outputfolder, file.replace(".jpg", ".desc")), desc)
        print(f"Processing {i+1}/{len(files)} frames")

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